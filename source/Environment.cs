/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
		public readonly int SelectionPoint;
		public readonly IGenomeFactory<TGenome> Factory;
		public readonly IProblem<TGenome> Problem;

		protected readonly ConcurrentQueue<TGenome> NewGenomes = new ConcurrentQueue<TGenome>();

		protected readonly ConcurrentDictionary<int, SortedList<IGenomeFitness<TGenome>, TGenome>>
			Generations = new ConcurrentDictionary<int, SortedList<IGenomeFitness<TGenome>, TGenome>>();

		protected readonly ConcurrentHashSet<string> Registry = new ConcurrentHashSet<string>();

		protected readonly ActionBlock<KeyValuePair<TGenome, int>> Reception;
		readonly ActionBlock<bool> FillEntryAction;

		protected Environment(IGenomeFactory<TGenome> genomeFactory, IProblem<TGenome> problem, int poolSize)
		{
			if (poolSize < MIN_POOL_SIZE)
				throw new ArgumentOutOfRangeException("poolSize", poolSize, "Must have a pool size of at least " + MIN_POOL_SIZE);

			PoolSize = poolSize;
			SelectionPoint = poolSize / 2 + 1;
			Factory = genomeFactory;
			Problem = problem;

			Reception = new ActionBlock<KeyValuePair<TGenome, int>>(async kvp =>
			{
				var result = await Problem.TestProcessor(kvp.Key, kvp.Value);
				var global = Problem.AddToGlobalFitness(kvp.Key, result);
				var list = GetGeneration(kvp.Value);
				ThreadSafety.SynchronizeWrite(list, () => list.Add(new GenomeFitness<TGenome>(kvp.Key, result), kvp.Key));

			}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 64 });

			TGenome latestTop = null;

			FillEntryAction = new ActionBlock<bool>(yes =>
			{
				Parallel.Invoke(
					() =>
					{
						var len = PoolSize * 3;
						// Keep the queue filled.
						for (var i = NewGenomes.Count; i < len; i++)
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
						};
					},

					() =>
					{

						// Work descening through the generations to make sure they flow.
						var gens = GenerationsWithEntries();
						var en = gens.GetEnumerator();
						if (en.MoveNext())
						{
							TGenome veryTop = ProcessGeneration(en.Current);
							var original = Interlocked.CompareExchange(ref latestTop, veryTop, latestTop);
							if (veryTop != null && veryTop!=original) TopGenome.Post(veryTop);
						}
						Parallel.ForEach(gens.Skip(1), e => ProcessGeneration(e));
					},
					() =>
					{
						var count = PoolSize;
						TGenome newGenome;
						while (--count >= 0 && NewGenomes.TryDequeue(out newGenome))
						{
							Reception.Post(new KeyValuePair<TGenome, int>(newGenome, 0));
						}
					}
				);

				// Keep it going...
				FillEntryAction.Post(true);
			});



			// Trick way to ensure only 1 instance is running.
			FillEntryAction.Post(true);
		}

		protected IEnumerable<KeyValuePair<int, SortedList<IGenomeFitness<TGenome>, TGenome>>> GenerationsWithEntries()
		{
			var gen = Generations.Count;
			for (var i = gen; i >= 0; i--)
			{
				SortedList<IGenomeFitness<TGenome>, TGenome> list;
				if (Generations.TryGetValue(i, out list))
				{
					if (list.Count > 0)
					{
						yield return new KeyValuePair<int, SortedList<IGenomeFitness<TGenome>, TGenome>>(i, list);
					}
				}
			}
		}

		TGenome ProcessGeneration(KeyValuePair<int, SortedList<IGenomeFitness<TGenome>, TGenome>> e)
		{
			var gen = e.Key;
			var genNext = gen + 1;
			var list = e.Value;
			TGenome top = null;
			if (list.Count >= PoolSize)
			{
				ThreadSafety.SynchronizeWrite(list, () =>
				{
					foreach (var d in list.Take(SelectionPoint).ToArray())
					{
						if (top == null) top = d.Value;
						list.Remove(d.Key);
						Reception.Post(new KeyValuePair<TGenome, int>(d.Value, genNext));
					}

					// Selection (culling) happens here:
					foreach (var d in list.Skip(list.Count - SelectionPoint).ToArray())
					{
						list.Remove(d.Key);
					}

				});
			}
			else
			{
				top = list.Select(kvp => kvp.Value).FirstOrDefault();
			}
			return top;
		}

		public int GenerationsCount
		{
			get
			{
				return Generations.Count;
			}
		}

		public int RegistryCount
		{
			get
			{
				return Registry.Count;
			}
		}

		protected bool Receive(TGenome genome)
		{
			if (genome != null && Registry.Add(genome.Hash))
			{
				NewGenomes.Enqueue(genome);
				return true;
			}
			return false;
		}

		SortedList<IGenomeFitness<TGenome>, TGenome> GetGeneration(int generation)
		{
			return Generations.GetOrAdd(generation, g =>
				new SortedList<IGenomeFitness<TGenome>, TGenome>(GenomeFitness.Comparer<TGenome>.Instance));
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
