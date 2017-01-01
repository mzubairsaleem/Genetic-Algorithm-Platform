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

		protected readonly ConcurrentDictionary<string, Lazy<GenomeFitness<TGenome, Fitness>>>
		Fitnesses = new ConcurrentDictionary<string, Lazy<GenomeFitness<TGenome, Fitness>>>();

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

		protected readonly BroadcastBlock<IGenomeFitness<TGenome>>
		NewTopGenome = new BroadcastBlock<IGenomeFitness<TGenome>>(null);

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
				if (genome != null)
				{
					// Ignore existing rejected...
					var keyed = GetFitnessForKeyTransform(genome);
					if (!Rejected.ContainsKey(genome.Hash) && (keyed == genome || !Rejected.ContainsKey(keyed.Hash)))
						TestBuffer.Post(genome);
				}
			});

			TestBuffer = new ActionBlock<TGenome>(
				ConsumeGenomeReadyForTesting,
				new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 16 });

			Task.Run(ConsumeCompletedTests);
		}

		async Task ConsumeGenomeReadyForTesting(TGenome genome)
		{
			var c = TestBuffer.InputCount;
			if (c > 10000) Console.WriteLine("Test Buffer: " + c);
			var gf = GetOrCreateFitnessFor(genome);
			var fitness = gf.Fitness;
			var storedGenome = gf.Genome;
			try
			{
				Interlocked.Increment(ref fitness.TestingCount);
				RankedPool.TryRemoveSynchronized(fitness);
				// ProcessTest should not have a different fitness object.
				// await Task.WhenAll(
				// 	Enumerable.Range(0,2).Select(i=>ProcessTest(genome, false)));
				await ProcessTest(gf); // Note the Genome contained here may not equal the genome parameter.
			}
			finally
			{
				// ** NOTE: We are only tracking 'if undergoing testing here.
				// Returning to the pool could be signaled more than once but we are attempting to prevent adds while values are changing.
				Interlocked.Decrement(ref fitness.TestingCount);
			}

			if (fitness.HasConverged())
			{
				ConvergentRegistry.Add(storedGenome);
				Converged.Post(storedGenome);
			}
			else
			{
				ConvergentRegistry.Remove(storedGenome);
			}

			TestCompleteBuffer.Post(genome);
		}

		async Task ConsumeCompletedTests()
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
						var gf = GetOrCreateFitnessFor(g);
						var fitness = gf.Fitness;
						ThreadSafety.SynchronizeReadWrite(fitness,
							lockType => fitness.TestingCount == 0 && !RankedPool.ContainsKey(fitness),
							() =>
							{
								RankedPool.Add(fitness, gf.Genome); // Return/Add the keyed version.
								count++;
							}); // **
					}
				});

				var top = TopGenome();
				if (top != null)
				{
					var gf = GetOrCreateFitnessFor(top);
					var fitness = gf.Fitness;
					// In some cases, what is used as the key ends up being another instance/version of it.
					var topHash = top.Hash;
					if (topHash != Interlocked.Exchange(ref LatestTopGenome, topHash))
						NewTopGenome.Post(new GenomeFitness<TGenome>(top, fitness.SnapShot()));
					Reception.Post(top); // Use original.

					var v = (TGenome)top.NextVariation();
					if (v != null) Reception.Post(v); // Use variation.

					Mutation.Post(top); // Use mutation.
				}

				// var bufferMax = 100;
				// while (TestBuffer.InputCount > bufferMax)
				// {
				// 	await Task.Yield();
				// 	bufferMax *= 10;
				// }

				// if (TestBuffer.InputCount > 100)
				// 	await Task.Yield();


				Generation.Post((uint)1);

				await CleanupAndGeneration(count); // Regenerate by the number of processed.
			}
		}

		protected abstract List<TGenome> RejectBadAndThenReturnKeepers(IEnumerable<GeneticAlgorithmPlatform.IGenomeFitness<TGenome, Fitness>> source, out List<Fitness> rejected);

		protected void StripRank(IEnumerable<Fitness> values)
		{
			ThreadSafety.SynchronizeWrite(RankedPool, () =>
			{
				foreach (var f in values) RankedPool.Remove(f);
			});
		}
		protected Task CleanupAndGeneration(uint count)
		{
			return Task.Run(() =>
			{
				List<Fitness> rejected;
				var dispursed = Triangular.Disperse.Decreasing(
						RejectBadAndThenReturnKeepers(
							ThreadSafety.SynchronizeRead(RankedPool, () => RankedPool.Select(kvp => kvp.GFFromFG()).ToArray()),
							out rejected
						)
					).ToArray();

				var len = dispursed.Length;
				if (len != 0)
				{
					for (uint i = 0; i < count; i++)
					{
						Reception.Post(dispursed.RandomSelectOne());
					}
				}

				StripRank(rejected);
			});
		}

		// Override this if there is a common key for multiple genomes (aka they are equivalient).
		protected virtual TGenome GetFitnessForKeyTransform(TGenome genome)
		{
			return genome;
		}

		protected bool TryGetFitnessFor(TGenome genome, out GenomeFitness<TGenome, Fitness> fitness)
		{
			genome = GetFitnessForKeyTransform(genome);
			var key = genome.Hash;
			Lazy<GenomeFitness<TGenome, Fitness>> value;
			if (Fitnesses.TryGetValue(key, out value))
			{
				fitness = value.Value;
				return true;
			}
			else
			{
				fitness = default(GenomeFitness<TGenome, Fitness>);
				return false;
			}
		}

		protected GenomeFitness<TGenome, Fitness>? GetFitnessFor(TGenome genome)
		{
			GenomeFitness<TGenome, Fitness> gf;
			TryGetFitnessFor(genome, out gf);
			return gf;
		}

		protected GenomeFitness<TGenome, Fitness> GetOrCreateFitnessFor(TGenome genome)
		{
			if (!genome.IsReadOnly)
				throw new InvalidOperationException("Cannot recall fitness for an unfrozen genome.");
			genome = GetFitnessForKeyTransform(genome);
			var key = genome.Hash;
			return Fitnesses
				.GetOrAdd(key, k =>
					Lazy.New(() => GenomeFitness.New(genome, new Fitness()))).Value;
		}

		public async Task WaitForConverged()
		{
			await Converged.OutputAvailableAsync().ConfigureAwait(false);
		}

		public Fitness TopFitness()
		{
			return ThreadSafety.SynchronizeRead(RankedPool,
				() => RankedPool.Keys.FirstOrDefault());
		}

		public virtual TGenome TopGenome()
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

		public List<GenomeFitness<TGenome>> Pareto(IEnumerable<TGenome> population = null)
		{
			var d = (population ?? Ranked())
				.Distinct()
				.Select(g =>
				{
					var gf = GetFitnessFor(g);
					return gf.HasValue ? GenomeFitness.New(g, gf.Value.Fitness) : gf;
				})
				.Where(g => g.HasValue)
				.Select(g => g.Value.SnapShot())
				.ToDictionary(g => g.Genome.Hash, g => g);

			bool found;
			List<GenomeFitness<TGenome>> p;
			do
			{
				found = false;
				p = d.Values.ToList();
				foreach (var g in p)
				{
					var gs = g.Fitness.Scores.ToArray();
					var len = gs.Length;
					if (d.Values.Any(o =>
						 {
							 var os = o.Fitness.Scores.ToArray();
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
						d.Remove(g.Genome.Hash);
					}
				}
			} while (found);

			return p;
		}

		protected abstract Task ProcessTest(GenomeFitness<TGenome, Fitness> g, bool useAsync = true);

		protected Task ProcessTest(TGenome g, bool useAsync = true)
		{
			return ProcessTest(GetOrCreateFitnessFor(g), useAsync);
		}

		public IDisposable[] Consume(IGenomeFactory<TGenome> source)
		{
			return new IDisposable[] {
				source.LinkReception(Reception),
				source.LinkMutation(Mutation),
				Generation.LinkTo(new ActionBlock<uint>(count=>source.Generate(count)))
				//source.LinkCrossover(Crossover)
			};
		}

		public IDisposable ListenToTopChanges(ITargetBlock<IGenomeFitness<TGenome>> target)
		{
			return NewTopGenome.LinkTo(target);
		}

	}
}