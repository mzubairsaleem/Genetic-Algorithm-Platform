using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open;
using Open.Arithmetic;
using Open.Collections;
using Open.Threading;

namespace GeneticAlgorithmPlatform
{
    public abstract class ProblemBase<TGenome> : IProblem<TGenome>
	where TGenome : class, IGenome
	{
		protected readonly SortedDictionary<Fitness, TGenome>
		RankedPool = new SortedDictionary<Fitness, TGenome>();

		protected readonly ConcurrentDictionary<string, Lazy<Fitness>>
		Fitnesses = new ConcurrentDictionary<string, Lazy<Fitness>>();

		protected readonly BufferBlock<TGenome>
		TestCompleteBuffer = new BufferBlock<TGenome>();

		protected readonly ConcurrentDictionary<string, bool>
		Rejected = new ConcurrentDictionary<string, bool>();

		protected readonly ConcurrentHashSet<TGenome>
		ConvergentRegistry = new ConcurrentHashSet<TGenome>();

		protected readonly BufferBlock<TGenome>
		Converged = new BufferBlock<TGenome>();

		protected readonly BufferBlock<TGenome>
		Mutation = new BufferBlock<TGenome>();

		protected readonly BufferBlock<uint>
		Generation = new BufferBlock<uint>();


		protected string LatestTopGenome;

		protected readonly BroadcastBlock<Tuple<TGenome, Fitness>>
		NewTopGenome = new BroadcastBlock<Tuple<TGenome, Fitness>>(null);

		protected readonly ActionBlock<TGenome> TestBuffer;

		protected readonly ActionBlock<TGenome> Reception;

		//protected readonly BufferBlock<TGenome> Crossover;

		// protected readonly TransformBlock<Tuple<TGenome,TGenome>, TGenome> Crossover;


		public TGenome[] Convergent
		{
			get
			{
				return ConvergentRegistry.ToArrayDirect();
			}
		}

		static int ProblemCount = 0;
		readonly int _id = Interlocked.Increment(ref ProblemCount);
		public int ID { get { return _id; } }

		public ProblemBase()
		{
			Reception = new ActionBlock<TGenome>(genome =>
			{
				// Ignore existing rejected...
				if (!Rejected.ContainsKey(genome.Hash))
					TestBuffer.Post(genome);
			});

			TestBuffer = new ActionBlock<TGenome>(async genome =>
			{
				var fitness = GetFitnessFor(genome);
				try
				{
					ThreadSafety.SynchronizeWrite(fitness, () => fitness.TestingCount++);
					RankedPool.TryRemoveSynchronized(fitness);
					// ProcessTest should not have a different fitness object.
					// await Task.WhenAll(
					// 	Enumerable.Range(0,2).Select(i=>ProcessTest(genome, false)));
					await ProcessTest(genome, false);
				}
				finally
				{
					// ** NOTE: We are only tracking 'if undergoing testing here.
					// Returning to the pool could be signaled more than once but we are attempting to prevent adds while values are changing.
					ThreadSafety.SynchronizeWrite(fitness, () => fitness.TestingCount--);
				}

				var key = genome.Hash;
				if (fitness.HasConverged())
				{
					ConvergentRegistry.Add(genome);
					Converged.Post(genome);
				}
				else
				{
					ConvergentRegistry.Remove(genome);
				}

				TestCompleteBuffer.Post(genome);
			});

			Action processTests = async () =>
			{
				while (await TestCompleteBuffer.OutputAvailableAsync())
				{
					IList<TGenome> list;
					if (!TestCompleteBuffer.TryReceiveAll(out list))
						continue;

					uint count = 0;
					ThreadSafety.SynchronizeWrite(RankedPool, () =>
					{
						foreach (var g in list.Distinct())
						{
							var fitness = GetFitnessFor(g);
							ThreadSafety.SynchronizeReadWrite(fitness,
								lockType => fitness.TestingCount == 0 && !RankedPool.ContainsKey(fitness),
								() =>
								{
									RankedPool.Add(fitness, g);
									count++;
								}); // **
						}
					});

					var top = TopGenome();
					var topHash = top.Hash;
					if (topHash != Interlocked.Exchange(ref LatestTopGenome, topHash))
						NewTopGenome.Post(new Tuple<TGenome, Fitness>(top, GetFitnessFor(top)));
					//Reception.Post(top); // Use original.
					Reception.Post((TGenome)top.NextVariation()); // Use variation.
					Mutation.Post(top); // Use mutation.
					Generation.Post((uint)1);

					CleanupAndGeneration(count);
				}
			};

			Task.Run(processTests);
			//Task.Run(processTests);

			// Task.Run(async ()=>{
			// 	while(true)
			// 	{

			// 		await Task.Yield();
			// 	}
			// });
		}

		protected abstract List<TGenome> RejectBadAndThenReturnKeepers(TGenome[] genomes);

		protected void StripRank(IEnumerable<Fitness> values)
		{
			ThreadSafety.SynchronizeWrite(RankedPool,()=>{
				foreach(var f in values) RankedPool.Remove(f);
			});
		}
		protected void CleanupAndGeneration(uint count)
		{
			var dispursed = Triangular.Disperse.Decreasing(
					RejectBadAndThenReturnKeepers(
						ThreadSafety.SynchronizeRead(RankedPool, () => RankedPool.Values.ToArray())
					)
				).ToArray();

			var len = dispursed.Length;
			if(len!=0) for (uint i = 0; i < count; i++)
			{
				Reception.Post(dispursed.RandomSelectOne());
			}
		}

		protected virtual Fitness GetFitnessFor(TGenome genome, bool createIfMissing = true)
		{
			if (!genome.IsReadOnly)
				throw new InvalidOperationException("Cannot recall fitness for an unfrozen genome.");
			var key = genome.Hash;
			if (createIfMissing)
				return Fitnesses
					.GetOrAdd(key, k =>
						Lazy.New(() => new Fitness())).Value;

			Lazy<Fitness> value;
			return Fitnesses
				.TryGetValue(key, out value)
				? value.Value
				: null;
		}

		public async Task WaitForConverged()
		{
			await Converged.OutputAvailableAsync();
		}

		public Fitness TopFitness()
		{
			return ThreadSafety.SynchronizeRead(RankedPool,
				() => RankedPool.Keys.FirstOrDefault());
		}

		public TGenome TopGenome()
		{
			return ThreadSafety.SynchronizeRead(RankedPool,
				() => RankedPool.Values.FirstOrDefault());
		}

		public TGenome[] TopGenomes(int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");
			return ThreadSafety.SynchronizeRead(RankedPool, () =>
				RankedPool.Values.Take(count).ToArray());
		}

		public TGenome[] Ranked()
		{
			return ThreadSafety.SynchronizeRead(RankedPool, () => RankedPool.Values.ToArray());
		}

		public List<TGenome> Pareto(IEnumerable<TGenome> population = null)
		{
			var d = (population ?? Ranked())
				.Distinct()
				.ToDictionary(g => g.ToString(), g => g);

			bool found;
			List<TGenome> p;
			do
			{
				found = false;
				p = d.Values.ToList();
				foreach (var g in p)
				{
					var gs = this.GetFitnessFor(g).Scores.ToArray();
					var len = gs.Length;
					if (d.Values.Any(o =>
						 {
							 var os = this.GetFitnessFor(o).Scores.ToArray();
							 for (var i = 0; i < len; i++)
							 {
								 var osv = os[i];
								 if (double.IsNaN(osv)) return true;
								 if (gs[i] <= os[i]) return false;
							 }
							 return true;
						 }))
					{
						found = true;
						d.Remove(g.Hash);
					}
				}
			} while (found);

			return p;
		}

		protected abstract Task<Fitness> ProcessTest(TGenome g, bool useAsync = true);

		public IDisposable[] Consume(IGenomeFactory<TGenome> source)
		{
			return new IDisposable[] {
				source.LinkReception(Reception),
				source.LinkMutation(Mutation),
				Generation.LinkTo(new ActionBlock<uint>(count=>source.Generate(count)))
				//source.LinkCrossover(Crossover)
			};
		}

		public IDisposable ListenToTopChanges(ITargetBlock<Tuple<TGenome, Fitness>> target)
		{
			return NewTopGenome.LinkTo(target);
		}

	}
}