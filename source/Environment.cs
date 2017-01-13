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

namespace GeneticAlgorithmPlatform
{

	// Defines the pipeline?
	public abstract class Environment<TGenome> : IEnvironment<TGenome>
		where TGenome : class, IGenome
	{
		const ushort MIN_POOL_SIZE = 2;

		public readonly BroadcastBlock<TGenome> TopGenome = new BroadcastBlock<TGenome>(null);
		public readonly ushort PoolSize;
		public readonly IGenomeFactory<TGenome> Factory;
		public readonly IProblem<TGenome> Problem;

		protected readonly GenomeProducer<TGenome> Producer;

		protected readonly ITargetBlock<TGenome> FinalistPool;

		protected readonly ISourceBlock<TGenome> Pipeline;

		protected readonly ActionBlock<TGenome> Breeders;

		protected readonly ActionBlock<TGenome> VipPool;

		const int ConvergenceThreshold = 20;

		protected Environment(
			IGenomeFactory<TGenome> genomeFactory,
			IProblem<TGenome> problem,
			ushort poolSize,
			uint networkDepth = 3,
			byte nodeSize = 2)
		{
			if (poolSize < MIN_POOL_SIZE)
				throw new ArgumentOutOfRangeException("poolSize", poolSize, "Must have a pool size of at least " + MIN_POOL_SIZE);

			PoolSize = poolSize;
			Factory = genomeFactory;
			Problem = problem;
			Producer = new GenomeProducer<TGenome>(Factory.Generate());

			var SecondChance = new BufferBlock<TGenome>();

			Breeders = new ActionBlock<TGenome>(genome => Producer.TryEnqueue(Breed(genome)),
				new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8 });

			var pipelineBuilder = new GenomePipelineBuilder<TGenome>(Producer, problem, poolSize, nodeSize, selected =>
			{
				var top = selected.FirstOrDefault();
				if (top != null) Breeders.SendAsync(top);
			});

			Pipeline = pipelineBuilder.CreateNetwork(networkDepth); // 3? Start small?

			Func<TGenome, bool> checkForConvergence = genome =>
			{
				var gf = problem.GetFitnessFor(genome).Value;
				var fitness = gf.Fitness;
				var count = fitness.SampleCount;
				if (fitness.HasConverged(ConvergenceThreshold))
				{
					TopGenome.Post(gf.Genome);
					Pipeline.Complete();
					return true;
				}
				return false;
			};


			VipPool = new ActionBlock<TGenome>(async genome =>
			{
				var fitness = problem.GetFitnessFor(genome).Value.Fitness;
				// You made it all the way back to the top?  Forget about what I said...
				fitness.RejectionCount = -1;
				if (fitness.HasConverged(0)) // 100 just to prove it.
				{
					if (!checkForConvergence(genome)) // should be enough for perfect convergence.
					{
						// Unseen data...
						Problem.AddToGlobalFitness(
							new GenomeFitness<TGenome>(genome, await problem.TestProcessor(genome, GenomePipeline.UniqueBatchID())));
						VipPool.Post(genome);
					}
				}
			}, new ExecutionDataflowBlockOptions()
			{
				MaxDegreeOfParallelism = 3
			});

			FinalistPool = pipelineBuilder.Selector(
				selection =>
				{
					var selected = selection.Selected;
					// Finalists use global fitness?
					var top = selected
						.OrderBy(g =>
							problem.GetFitnessFor(g, true).Value,
							GenomeFitness.Comparer<TGenome, Fitness>.Instance)
						.ThenByDescending(g => g.Hash.Length) // Might be equals.
						.FirstOrDefault();

					// NOTE: That global GenomeFitness returns may return a 'version' of the actual genome.
					// Ensure the global pareto is retained. (note is using global version)
					var paretoGenomes = GenomeFitness.Pareto(Problem.GetFitnessFor(selection.All)).Select(g => g.Genome).ToArray();

					if (top != null)
					{
						TopGenome.SendAsync(top);
						checkForConvergence(top);
						VipPool.SendAsync(top);
						// Top get's special treatment.
						for (var i = 0; i < networkDepth - 1; i++)
							Breeders.SendAsync(top);

						Task.Run(() =>
						{
							// Crossover.
							TGenome[] o2 = Factory.AttemptNewCrossover(top, Triangular.Disperse.Decreasing(selected).ToArray());
							if (o2 != null && o2.Length != 0)
								Producer.TryEnqueue(o2.Select(o => Problem.GetFitnessFor(o)?.Genome ?? o)); // Get potential stored variation.

							// Keep trying to breed pareto genomes since they conversely may have important genetic material.
							Producer.TryEnqueue(Factory.AttemptNewCrossover(paretoGenomes));

						});

					}

					// The top final pool recycles it's winners.
					foreach (var g in selected.Concat(paretoGenomes).Distinct()) //Also avoid re-entrance if there are more than one.
						FinalistPool.Post(g); // Might need to look at the whole pool and use pareto to retain.

					var rejected = new HashSet<TGenome>(selection.Rejected);
					rejected.ExceptWith(paretoGenomes);

					// Just in case a challenger got lucky.
					foreach (var reject in Problem.GetFitnessFor(rejected, true))
					{
						if (reject.Fitness.IncrementRejection() <= 1)
							Producer.TryEnqueue(reject.Genome, true);
						// else
						// 	Console.WriteLine("2nd Round Rejection: "+reject.Genome);
					}

				})
				.PropagateFaultsTo(TopGenome)
				.PropagateCompletionTo(TopGenome, VipPool, Breeders, Pipeline);

			Pipeline.LinkToWithExceptions(FinalistPool);
			Pipeline
				.PropagateCompletionTo(Producer, FinalistPool, SecondChance)
				.OnComplete(() => Console.WriteLine("Pipeline COMPLETED"));

			Producer
				.ProductionCompetion
				.ContinueWith(task => Pipeline.Fault("Producer Completed Unexpectedly."));

		}

		protected virtual IEnumerable<TGenome> Breed(TGenome genome)
		{
			var m = Factory.GenerateOne(genome);
			if (m != null) yield return m;
			var v = (TGenome)genome.NextVariation();
			if (v != null) yield return v;
		}


		// public Task<TGenome> AddProblem(IProblem<TGenome> problem)
		// {
		// 	problem.Consume(Factory);
		// 	problem.ListenToTopChanges(new ActionBlock<IGenomeFitness<TGenome>>(gf=>
		// 		TopChanges.Post(new Tuple<IProblem<TGenome>,IGenomeFitness<TGenome>>(problem,gf))
		// 	));
		// 	problem.WaitForConverged()
		// 		.ContinueWith(task => Converged.Post(problem));
		// }

		// private class ReverseIntComparer : IComparer<int>
		// {
		// 	public int Compare(int a, int b)
		// 	{
		// 		return b - a;
		// 	}
		// }

	}


}
