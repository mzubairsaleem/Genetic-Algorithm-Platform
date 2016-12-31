using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open.Collections;
using Open.Threading;

namespace GeneticAlgorithmPlatform
{
	public abstract class ProblemBase<TGenome> : IProblem<TGenome>
	where TGenome : class, IGenome
	{
		protected readonly SortedList<Fitness, TGenome>
			RankedPool = new SortedList<Fitness, TGenome>();

		protected readonly ConcurrentDictionary<string, Lazy<Fitness>>
			Fitnesses = new ConcurrentDictionary<string, Lazy<Fitness>>();

		protected readonly ConcurrentDictionary<string, bool>
			Rejected = new ConcurrentDictionary<string, bool>();

		protected readonly ConcurrentHashSet<TGenome>
			ConvergentRegistry = new ConcurrentHashSet<TGenome>();

		protected readonly BufferBlock<TGenome>
			Converged = new BufferBlock<TGenome>();

		protected readonly ActionBlock<TGenome> TestBuffer;

		protected readonly ActionBlock<TGenome> Reception;

		protected readonly BufferBlock<TGenome> Mutation;

		//protected readonly BufferBlock<TGenome> Crossover;

		protected readonly TransformBlock<Tuple<TGenome,TGenome>, TGenome> Crossover;


		public TGenome[] Convergent
		{
			get
			{
				return ConvergentRegistry.ToArrayDirect();
			}
		}


		protected readonly BufferBlock<TGenome> ReorderBuffer;

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
				var fitness = await ProcessTest(genome);
				if (Debugger.IsAttached)
					// ProcessTest should not have a different fitness object.
					Debug.Assert(fitness == GetFitnessFor(genome));

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

				ReorderBuffer.Post(genome);
			});

			Task.Run(async () =>
			{
				while (await ReorderBuffer.OutputAvailableAsync())
				{
					IList<TGenome> list;
					ReorderBuffer.TryReceiveAll(out list);
					ReorderRanking(list);

					var top = TopGenome();
					Reception.Post(top); // Use original.
					Reception.Post((TGenome)top.NextVariation()); // Use variation.
					Mutation.Post(top); // Use mutation.
				}
			});
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

		public TGenome TopGenome()
		{
			return ThreadSafety.SynchronizeRead(RankedPool, () =>
				RankedPool.Values.FirstOrDefault());
		}

		public TGenome[] TopGenomes(int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException("count");
			return ThreadSafety.SynchronizeRead(RankedPool, () =>
				RankedPool.Values.Take(count).ToArray());
		}

		protected bool ReorderRanking(TGenome genome)
		{
			var fitness = GetFitnessFor(genome);
			return RankedPool.TryRemoveSynchronized(fitness)
				&& !Rejected.ContainsKey(genome.Hash)
				&& RankedPool.TryAddSynchronized(fitness, genome);
		}

		protected void ReorderRanking(IEnumerable<TGenome> genomes)
		{
			ThreadSafety.SynchronizeWrite(RankedPool, () =>
			{
				foreach (var genome in genomes)
				{
					var fitness = GetFitnessFor(genome);
					RankedPool.Remove(fitness);
					if (!Rejected.ContainsKey(genome.Hash))
						RankedPool.Add(fitness, genome);
				}
			});
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

		public void Consume(IGenomeFactory<TGenome> source)
		{
			source.LinkReception(Reception);
			source.LinkMutation(Mutation);
			//source.LinkCrossover(Crossover);
		}

	}
}