/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open.Collections;
using Open.Threading;

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
		readonly ConcurrentDictionary<int, PoolProcessor<TGenome>> Generations = new ConcurrentDictionary<int, PoolProcessor<TGenome>>();
		readonly SortedDictionary<int, PoolProcessor<TGenome>>
			ProcessorsReadyForNext = new SortedDictionary<int, PoolProcessor<TGenome>>(new ReverseIntComparer());
		readonly ConcurrentHashSet<string> Registry = new ConcurrentHashSet<string>();

		protected Environment(IGenomeFactory<TGenome> genomeFactory, IProblem<TGenome> problem, int poolSize)
		{
			if (poolSize < MIN_POOL_SIZE)
				throw new ArgumentOutOfRangeException("poolSize", poolSize, "Must have a pool size of at least " + MIN_POOL_SIZE);

			PoolSize = poolSize;
			Factory = genomeFactory;
			Problem = problem;

			FillEntryQueue();
		}

		void StartNext()
		{
			FillEntryQueue(); // Ensure a waiting queue.

			PoolProcessor<TGenome> next = null;
			ThreadSafety.LockConditional(
				ProcessorsReadyForNext,
				() => ProcessorsReadyForNext.Count != 0,
				() =>
				{
					var first = ProcessorsReadyForNext.First();
					if(ProcessorsReadyForNext.Remove(first.Key))
						next = first.Value;
				}
			);

			if (next != null)
				next.Next();
		}

		PoolProcessor<TGenome> FillEntryQueue()
		{
			var genQueue = GetProcessorFor(0);

			// Keep the queue filled.
			Parallel.For(genQueue.Queued, PoolSize * 2, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, i =>
			{
				int attempts = 0;
				while (!Receive(Factory.Generate()))
				{
					attempts++;
					if (attempts == 20)
					{
						throw new Exception("Unable to create new genomes.");
					}
				}
			});

			return genQueue;
		}

		protected bool Receive(TGenome genome, int generation)
		{
			if (genome != null && Registry.Add(genome.Hash))
			{
				GetProcessorFor(generation).Post(genome);
				return true;
			}
			return false;
		}


		public bool Receive(TGenome genome)
		{
			return Receive(genome, 0);
		}

		int MaxReportedGeneration = 0;



		protected PoolProcessor<TGenome> GetProcessorFor(int generation)
		{
			return Generations.GetOrAdd(generation, key =>
			{
				var p = new PoolProcessor<TGenome>(Problem, PoolSize);
				p.Selected.LinkTo(new ActionBlock<TGenome[]>(results =>
				{
					var first = results.FirstOrDefault();
					if (first != null)
					{
						// It's quite possible to get a fitness generation out of sync here. For now, that's ok.
						if (ThreadSafety.InterlockedExchangeIfLessThanComparison(ref MaxReportedGeneration, generation, generation))
							TopGenome.Post(first);

						OnNextTop(first);
					}

					// Redistrubute.
					foreach (var genome in results)
					{
						GetProcessorFor(Problem.GetSampleCountFor(genome)).Post(genome);
					}

					// Reactivate.
					lock (ProcessorsReadyForNext)
					{
						ProcessorsReadyForNext.Add(generation, p);
					}

					StartNext();

				}));
				return p;
			});
		}



		protected virtual void OnNextTop(TGenome genome)
		{
			Receive(Factory.Generate(genome));
			Receive((TGenome)genome.NextVariation());
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

		private class ReverseIntComparer : IComparer<int>
		{
			public int Compare(int a, int b)
			{
				return b - a;
			}
		}

	}


}
