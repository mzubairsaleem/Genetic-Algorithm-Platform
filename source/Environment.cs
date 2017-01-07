/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GeneticAlgorithmPlatform
{

	// Defines the pipeline?
	public abstract class Environment<TGenome> : IEnvironment<TGenome>
		where TGenome : class, IGenome
	{
		const int MIN_POOL_SIZE = 2;

		public readonly BroadcastBlock<TGenome> TopGenome = new BroadcastBlock<TGenome>(null);
		public readonly int PoolSize;
		public readonly IGenomeFactory<TGenome> Factory;
		public readonly IProblem<TGenome> Problem;
		readonly List<IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]>> Generations = new List<IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]>>();

		readonly IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]> EntryPoint;
		readonly ActionBlock<GenomeFitness<TGenome>[]> FinishedTestingBatch;
		readonly ExecutionDataflowBlockOptions DefaultParallelOptions = new ExecutionDataflowBlockOptions()
		{
			MaxDegreeOfParallelism = 32,
			MaxMessagesPerTask = 3
		};

		readonly BufferBlock<TGenome> Reception = new BufferBlock<TGenome>();
		int BufferCount = 0;

		int MaxGeneration = 0;

		protected Environment(IGenomeFactory<TGenome> genomeFactory, IProblem<TGenome> problem, int poolSize)
		{
			if (poolSize < MIN_POOL_SIZE)
				throw new ArgumentOutOfRangeException("poolSize", poolSize, "Must have a pool size of at least " + MIN_POOL_SIZE);

			PoolSize = poolSize;
			Factory = genomeFactory;
			Problem = problem;

			FinishedTestingBatch = new ActionBlock<GenomeFitness<TGenome>[]>(
				gfs =>
				{
					// *** SELECTION OCCURS HERE ***
					var len = gfs.Length / 2 + 1;
					for (var i = 0; i < len; i++)
					{
						var gf = gfs[i];
						var fitness = Problem.AddToGlobalFitness(gf);
						var genome = gf.Genome;
						var sc = fitness.SampleCount;
						var mg = MaxGeneration;
						var newGen = sc >= mg;
						var processor = EnsureProcessor(sc);
						processor.Post(genome);

						if (i == 0)
						{
							// Top genomes deserve additional consideration.
							Task.Run(()=>OnNextTop(genome));
							if (newGen) TopGenome.Post(genome);
						}
					}
				},
				DefaultParallelOptions);

			EntryPoint = EnsureProcessor(0);

			Reception.LinkTo(new ActionBlock<TGenome>(genome =>
			{
				var bc = Interlocked.Decrement(ref BufferCount);
				// Console.WriteLine("BC: "+bc);
				if (bc < poolSize * 2)
				{
					for (var i = 0; i < 3; i++)
						Receive(Factory.Generate());
				}
				EntryPoint.Post(genome);

			}, new ExecutionDataflowBlockOptions
			{
				MaxDegreeOfParallelism = 6
			}));
			// Kick it off...
			Receive(Factory.Generate());
		}

		protected void Receive(TGenome genome)
		{
			if (genome != null)
			{
				Interlocked.Increment(ref BufferCount);
				Reception.Post(genome);
			}
		}

		protected virtual void OnNextTop(TGenome genome)
		{
			Receive(Factory.Generate(genome));
			Receive((TGenome)genome.NextVariation());
		}

		IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]> EnsureProcessor(int index)
		{
			if (Generations.Count <= index)
			{
				lock (Generations)
				{
					var updated = false;
					while (Generations.Count <= index)
					{
						var p = PoolProcessor.GenerateTransform(PoolSize, Problem.TestProcessor);
						Generations.Add(p);
						p.LinkTo(FinishedTestingBatch);
						updated = true;
					}
					if (updated)
						Interlocked.Exchange(ref MaxGeneration, Generations.Count);
				}
			}
			return Generations[index];
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

	}


}
