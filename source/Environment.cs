/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
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

		protected Environment(IGenomeFactory<TGenome> genomeFactory, IProblem<TGenome> problem, ushort poolSize, byte nodeSize = 2, uint networkDepth = 4)
		{
			if (poolSize < MIN_POOL_SIZE)
				throw new ArgumentOutOfRangeException("poolSize", poolSize, "Must have a pool size of at least " + MIN_POOL_SIZE);

			PoolSize = poolSize;
			Factory = genomeFactory;
			Problem = problem;
			Producer = new GenomeProducer<TGenome>(Factory.Generator());

			var pipelineBuilder = new GenomePipelineBuilder<TGenome>(Producer, problem, poolSize, nodeSize, selected =>
			{
				var top = selected.FirstOrDefault();
				if (top != null)
				{
					foreach (var offspring in Breed(top))
						Producer.TryEnqueue(offspring);
				}
			});

			var pipeline = pipelineBuilder.CreateNetwork(networkDepth); // 3? Start small?

			ActionBlock<TGenome> vipPool = null;
			vipPool = new ActionBlock<TGenome>(async genome =>
			{
				var fitness = problem.GetFitnessFor(genome).Value.Fitness;
				var count = fitness.SampleCount;
				if (fitness.HasConverged(0)) // 100 just to prove it.
				{
					if (count < 100)
					{
						// Unseen data...
						await problem.TestProcessor(genome, GenomePipeline.UniqueBatchID());
						vipPool.Post(genome);
					}
					else
					{
						TopGenome.Post(genome);
						TopGenome.Complete();
						vipPool.Complete();
						FinalistPool.Complete();
						pipeline.Complete();
						Producer.Complete();
					}
				}

			});

			FinalistPool = pipelineBuilder.Distributor(selected =>
			{
				var top = selected.FirstOrDefault();
				if (top != null)
				{
					TopGenome.Post(top);
					vipPool.Post(top);

					foreach (var offspring in Breed(top))
						Producer.TryEnqueue(offspring);
				}

				// The top final pool recycles it's winners.
				foreach (var g in selected)
					FinalistPool.Post(g);
			});

			pipeline.LinkTo(FinalistPool);

		}

		protected virtual IEnumerable<TGenome> Breed(TGenome genome)
		{
			var m = Factory.Generate(genome);
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
