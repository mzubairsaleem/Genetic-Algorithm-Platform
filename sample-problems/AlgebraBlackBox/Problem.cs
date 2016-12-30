/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using AlgebraBlackBox.Genes;
using Open;
using Open.Arithmetic;
using Open.Collections;
using Open.Formatting;
using Open.Threading;
using Fitness = GeneticAlgorithmPlatform.Fitness;

namespace AlgebraBlackBox
{

	public delegate double Formula(params double[] p);

	///<summary>
	/// The 'Problem' class is important for tracking fitness results and deciding how well a genome is peforming.
	/// It's possible to have multiple 'problems' being measured at once so each Problem class has to keep a rank of the genomes.
	///</summary>
	public sealed class Problem : GeneticAlgorithmPlatform.IProblem<Genome>
	{

		Formula _actualFormula;


		SortedList<Fitness, Genome> _rankedPool;

		// We use a lazy to ensure 100% concurrency since ConcurrentDictionary is optimistic;
		ConcurrentDictionary<string, Lazy<Fitness>> _fitness;
		ConcurrentDictionary<string, Genome> _convergent;
		ConcurrentDictionary<string, bool> _rejected;

		ConcurrentQueue<Fitness> _reorderQueue;

		public Problem(Formula actualFormula)
		{
			_rankedPool = new SortedList<Fitness, Genome>();
			_fitness = new ConcurrentDictionary<string, Lazy<Fitness>>();
			_actualFormula = actualFormula;
			_convergent = new ConcurrentDictionary<string, Genome>();
		}

		public Fitness GetFitnessFor(Genome genome, bool createIfMissing = true)
		{
			if (!genome.IsReadOnly)
				throw new InvalidOperationException("Cannot recall fitness for an unfrozen genome.");
			var reduced = genome.AsReduced();
			var key = reduced.ToString();
			if (createIfMissing) return _fitness.GetOrAdd(key, k => Lazy.New(() =>
			{
				var f = new Fitness();
				_rankedPool.TryAddSynchronized(f, reduced); // Used to feed next in rank or claim for testing.
				return f;
			})).Value;

			Lazy<Fitness> value;
			return _fitness.TryGetValue(key, out value) ? value.Value : null;
		}

		public ICollection<Genome> Convergent
		{
			get
			{
				return this._convergent.Values;
			}
		}

		public Genome[] Ranked()
		{
			return ThreadSafety.SynchronizeRead(_rankedPool, () => _rankedPool.Values.ToArray());
		}

		public IEnumerable<Genome> Rank(IEnumerable<Genome> population)
		{
			return population
				//.AsParallel()
				.Select(g => new
				{
					Genome = g,
					Fitness = GetFitnessFor(g)
				})
				.Where(g => g.Fitness.Count > 0)
				.OrderBy(g => g.Fitness)
				.ThenBy(g => g.Genome.Hash.Length)
				.Select(g => g.Genome);
		}

		public List<Genome> Pareto(IEnumerable<Genome> population)
		{
			// TODO: Needs work/optimization.
			var d = population
				.Select(g => g.AsReduced())
				.Distinct()
				.ToDictionary(g => g.AsReduced().ToString(), g => g);

			bool found;
			List<Genome> p;
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

		public Genome TakeNextTopGenome()
		{
			return PeekNextTopGenome((kvp, lt) =>
				// We have a write lock? YES! Remove from list! Return false if not possible so that the take returns null.
				lt == LockType.Write
					? kvp.HasValue && _rankedPool.Remove(kvp.Value.Key)
					: true);
		}

		// When upgradeLockCondition returns true the lock will progressively be upgraded from Read, ReadUpgradeable, to Write.
		Genome PeekNextTopGenome(Func<KeyValuePair<Fitness, Genome>?, LockType, bool> upgradeLockCondition = null)
		{
			KeyValuePair<Fitness, Genome>? kvp = null;
			Func<LockType, bool> condition = lockType =>
			{
				switch (lockType)
				{
					case LockType.Read:
					// LockType.Read: This is just an invalidation test. The lock will be lost after this and then reacquired as an upgradable read lock.
					case LockType.ReadUpgradeable:
						var e = _rankedPool.GetEnumerator();
						if (e.MoveNext()) kvp = e.Current;
						else kvp = null;
						return upgradeLockCondition == null ? false : upgradeLockCondition(kvp, lockType);
					case LockType.Write:
						// If we get this far, then 'g' is already acquired and we don't need to re-create a new enumerator.
						var c = upgradeLockCondition(kvp, lockType);
						if(!c) kvp = null; // If the above condition returns false, then that's a signal for failure and we should return null at the end.
						return c;
				}
				return false; // Should never occur because there are only 3 lock types.
			};
			ThreadSafety.SynchronizeReadWrite(_rankedPool,
				condition, () => { condition(LockType.Write); });

			return kvp.HasValue ? kvp.Value.Value : null;
		}

		

		void ReorderRanking(Fitness fitness, Genome genome)
		{
			if( _rankedPool.TryRemoveSynchronized(fitness))
				ReturnGenomeToRanking(fitness,genome);				
		}

		public void ReorderRanking(Genome genome)
		{
			ReorderRanking(GetFitnessFor(genome),genome);
		}

		void ReturnGenomeToRanking(Fitness fitness, Genome genome)
		{
			if(!_rankedPool.TryAddSynchronized(fitness,genome))
				throw new Exception("Could not return (add) a genome to ranking.");
		}

		public void ReturnGenomeToRanking(Genome genome)
		{
			ReturnGenomeToRanking(GetFitnessFor(genome),genome);
		}


		//noinspection JSMethodCanBeStatic
		double[] Sample(int count = 5, double range = 100)
		{
			var result = new HashSet<double>();

			while (result.Count < count)
			{
				result.Add(RandomUtilities.Random.NextDouble() * range);
			}
			return result.OrderBy(v => v).ToArray();
		}


		Task ProcessTestAsync(IEnumerable<Genome> genomes)
		{
			return Task.WhenAll(
				TestPrep((correct, samples) =>
					genomes.Select(g => ProcessTest(g, correct, samples, true))
			).ToArray());
		}

		async Task<Fitness> ProcessTest(Genome g, List<double> correct, List<double[]> samples, bool useAsync)
		{
			var fitness = GetFitnessFor(g);
			var len = correct.Count;
			var divergence = new double[correct.Count];
			var calc = new double[correct.Count];
			var NaNcount = 0;

			#if DEBUG
			var gRed = g.AsReduced();
			#endif

			for (var i = 0; i < len; i++)
			{
				var s = samples[i];
				var result = useAsync ? await g.CalculateAsync(s) : g.Calculate(s);
				#if DEBUG
				if (gRed != g)
				{
					var rr = useAsync ? await gRed.CalculateAsync(s) : gRed.Calculate(s);
					if (!g.Genes.OfType<ParameterGene>().Any(gg => gg.ID > 1) // For debugging/testing IDs greater than 1 are invalid so ignore.
						&& !result.IsRelativeNearEqual(rr, 7))
					{
						var message = String.Format(
							"Reduction calculation doesn't match!!! {0} => {1}\n\tSample: {2}\n\tresult: {3} != {4}",
							g, gRed, s.JoinToString(", "), result, rr);
						if (!result.IsNaN())
							Debug.Fail(message);
						else
							Debug.WriteLine(message);
					}
				}
				#endif
				if (double.IsNaN(result)) NaNcount++;
				calc[i] = result;
				divergence[i] = -Math.Abs(result - correct[i]);
			}

			if (NaNcount != 0)
			{
				// We do not yet handle NaN values gracefully yet so avoid correlation.
				fitness.AddScores(
					NaNcount == len // All NaN basically = fail.  Don't waste time trying to correlate.
						? double.NegativeInfinity
						: -2,
					double.NegativeInfinity);
				ReorderRanking(fitness,g);
				return fitness;
			}

			var c = correct.Correlation(calc);
			var d = divergence.Average() + 1;

			fitness.AddScores(
				(double.IsNaN(c) || double.IsInfinity(c)) ? -2 : c,
				(double.IsNaN(d) || double.IsInfinity(d)) ? double.NegativeInfinity : d
			);

			ReorderRanking(fitness,g); // This could be deferred?  Queued?

			var key = g
				.AsReduced()
				.ToString();
			if (fitness.HasConverged())
			{
				_convergent[key] = g;
			}
			else
			{
				Genome v;
				_convergent.TryRemove(key, out v);
			}

			return fitness;
		}

		void ProcessTest(IEnumerable<Genome> genomes)
		{
			TestPrep(
				(correct, samples) =>
				{
					Parallel.ForEach(genomes, g => ProcessTest(g, correct, samples, false).RunSynchronously());
					return true;
				});
		}

		T TestPrep<T>(Func<List<double>, List<double[]>, T> handler)
		{
			// Need a way to diversify the number of parameter samples...
			var f = this._actualFormula;

			var aSample = Sample();
			var bSample = Sample();
			var samples = new List<double[]>();
			var correct = new List<double>();

			foreach (var a in aSample)
			{
				foreach (var b in bSample)
				{
					samples.Add(new double[] { a, b });
					correct.Add(f(a, b));
				}
			}

			if (Debugger.IsAttached && correct.All(v => double.IsNaN(v)))
				throw new Exception("Formula cannot render allways NaN.");

			return handler(correct, samples);
		}

		IEnumerable<Task> ProcessTestAsync(IEnumerable<Genome> p, int count)
		{
			for (var i = 0; i < count; i++)
			{
				yield return ProcessTestAsync(p);
			}
		}

		void ProcessTest(IEnumerable<Genome> p, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ProcessTest(p);
			}
		}

		public Task TestAsync(IEnumerable<Genome> p, int count = 1)
		{
			if (p == null)
				throw new ArgumentNullException("p");

			// TODO: Need to find a way to dynamically implement more than 2 params... (discover significant params)
			return Task.WhenAll(ProcessTestAsync(p, count));
		}

		public void GetConvergent(BufferBlock<Genome> queue)
		{
			throw new NotImplementedException();
		}

		public void Test(IEnumerable<Genome> p, int count = 1)
		{
			if (p == null)
				throw new ArgumentNullException("p");

			// TODO: Need to find a way to dynamically implement more than 2 params... (discover significant params)
			ProcessTest(p, count);
		}


		/*
			// For the most part is not as performant when low sample count and short calculation times.
			// But keep this here for reference.
			Task<Fitness> ParallelTest(Genome g, List<double> correct, List<double[]> samples)
			{
				var len = correct.Count;
				var divergence = new double[correct.Count];
				var calc = new double[correct.Count];

				return Task

					.WhenAll(
						Enumerable.Range(0, len)
						.Select(i =>
							g.Calculate(samples[i])
								.ContinueWith(task =>
								{
									var result = task.Result;
									calc[i] = result;
									divergence[i] = -Math.Abs(result - correct[i]);
								}))
						.ToArray())

					.ContinueWith(task =>
					{
						var c = correct.Correlation(calc);
						var d = divergence.Average() + 1;

						var fitness = GetFitnessFor(g);
						fitness.AddScores(
							(double.IsNaN(c) || double.IsInfinity(c)) ? -2 : c,
							(double.IsNaN(d) || double.IsInfinity(d)) ? double.NegativeInfinity : d
						);

						Task.Run(() =>
						{
							var key = g
								.AsReduced()
								.ToString();
							if (fitness.HasConverged())
							{
								_convergent[key] = g;
							}
							else
							{
								Genome v;
								_convergent.TryRemove(key, out v);
							}
						});

						return fitness;
					});
			}
		*/


	}



}