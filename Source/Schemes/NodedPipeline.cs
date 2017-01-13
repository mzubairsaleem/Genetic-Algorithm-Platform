/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open.Arithmetic;
using Open.DataFlow;

namespace GeneticAlgorithmPlatform.Schemes
{

	// Defines the pipeline?
	public sealed class NodedPipeline<TGenome> : EnvironmentBase<TGenome>
		where TGenome : class, IGenome
	{
		public readonly BroadcastBlock<TGenome> TopGenome = new BroadcastBlock<TGenome>(null);

		readonly GenomeProducer<TGenome> Producer;

		readonly ITargetBlock<TGenome> FinalistPool;

		readonly GenomePipelineBuilder<TGenome> PipelineBuilder;

		readonly ISourceBlock<TGenome> Pipeline;

		readonly ActionBlock<TGenome> Breeders;

		readonly ActionBlock<TGenome> VipPool;

		const int ConvergenceThreshold = 20;

		public NodedPipeline(
			IGenomeFactory<TGenome> genomeFactory,
			ushort poolSize,
			uint networkDepth = 3,
			byte nodeSize = 2) : base(genomeFactory, poolSize)
		{
			if (poolSize < MIN_POOL_SIZE)
				throw new ArgumentOutOfRangeException("poolSize", poolSize, "Must have a pool size of at least " + MIN_POOL_SIZE);

			Producer = new GenomeProducer<TGenome>(Factory.Generate());

			Breeders = new ActionBlock<TGenome>(genome => Producer.TryEnqueue(Factory.Expand(genome)),
				new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8 });

			PipelineBuilder = new GenomePipelineBuilder<TGenome>(Producer, Problems, poolSize, nodeSize, selected =>
			{
				var top = selected.FirstOrDefault();
				if (top != null) Breeders.SendAsync(top);
			});

			Pipeline = PipelineBuilder.CreateNetwork(networkDepth); // 3? Start small?

			Func<TGenome, bool> checkForConvergence = genome =>
			{
				foreach (var problem in Problems)
				{
					var gf = problem.GetFitnessFor(genome).Value;
					var fitness = gf.Fitness;
					var count = fitness.SampleCount;
					if (fitness.HasConverged(ConvergenceThreshold))
					{
						TopGenome.Post(gf.Genome);
						TopGenome.Complete();
						Pipeline.Complete();
						return true;
					}
				}
				return false;
			};


			VipPool = new ActionBlock<TGenome>(async genome =>
			{
				foreach (var problem in Problems)
				{
					var fitness = problem.GetFitnessFor(genome).Value.Fitness;
					// You made it all the way back to the top?  Forget about what I said...
					fitness.RejectionCount = -1;
					if (fitness.HasConverged(0)) // 100 just to prove it.
					{
						if (!checkForConvergence(genome)) // should be enough for perfect convergence.
						{
							// Unseen data...
							problem.AddToGlobalFitness(
								new GenomeFitness<TGenome>(genome, await problem.TestProcessor(genome, GenomePipeline.UniqueBatchID())));
							VipPool.Post(genome);
						}
					}
				}

			}, new ExecutionDataflowBlockOptions()
			{
				MaxDegreeOfParallelism = 3
			});

			FinalistPool = PipelineBuilder.Selector(
				selection =>
				{
					var selected = selection.Selected;
					// Finalists use global fitness?
					// Get the top one for each problem.
					var top = Problems.Select(p =>
						selected
							.OrderBy(g =>
								p.GetFitnessFor(g, true).Value,
								GenomeFitness.Comparer<TGenome, Fitness>.Instance)
							.ThenByDescending(g => g.Hash.Length) // Might be equals.
							.FirstOrDefault())
							.Where(g => g != null)
							.Distinct();

					// NOTE: That global GenomeFitness returns may return a 'version' of the actual genome.
					// Ensure the global pareto is retained. (note is using global version)
					var paretoGenomes = Problems.SelectMany(p =>
							GenomeFitness.Pareto(p.GetFitnessFor(selection.All)).Select(g => g.Genome)
						)
						.Distinct()
						.ToArray();

					foreach (var t in top)
					{
						TopGenome.SendAsync(t);
						checkForConvergence(t);
						VipPool.SendAsync(t);
						// Top get's special treatment.
						for (var i = 0; i < networkDepth - 1; i++)
							Breeders.SendAsync(t);

						// Crossover.
						TGenome[] o2 = Factory.AttemptNewCrossover(t, Triangular.Disperse.Decreasing(selected).ToArray());
						if (o2 != null && o2.Length != 0)
						{
							foreach (var problem in Problems)
							{
								Producer.TryEnqueue(o2.Select(o => problem.GetFitnessFor(o)?.Genome ?? o)); // Get potential stored variation.
							}
						}

						// Keep trying to breed pareto genomes since they conversely may have important genetic material.
						Producer.TryEnqueue(Factory.AttemptNewCrossover(paretoGenomes));

					}

					// The top final pool recycles it's winners.
					foreach (var g in selected.Concat(paretoGenomes).Distinct()) //Also avoid re-entrance if there are more than one.
						FinalistPool.Post(g); // Might need to look at the whole pool and use pareto to retain.

					var rejected = new HashSet<TGenome>(selection.Rejected);
					rejected.ExceptWith(paretoGenomes);

					// Just in case a challenger got lucky.
					Producer.TryEnqueue(
						Problems
							.SelectMany(p => p.GetFitnessFor(rejected, true))
							.Where(gf => gf.Fitness.IncrementRejection() <= 1)
							.Select(gf => gf.Genome)
							.Distinct(), true);

				})
				.PropagateFaultsTo(TopGenome)
				.PropagateCompletionTo(TopGenome, VipPool, Breeders, Pipeline);

			Pipeline.LinkToWithExceptions(FinalistPool);
			Pipeline.PropagateCompletionTo(Producer)
				.OnComplete(() => Console.WriteLine("Pipeline COMPLETED"));

			Producer
				.ProductionCompetion
				.ContinueWith(task => Pipeline.Fault("Producer Completed Unexpectedly."));

		}

		protected override Task StartInternal()
		{
			var completed = Pipeline.Completion;
			Producer.Poke();
			return completed;
		}
	}


}
