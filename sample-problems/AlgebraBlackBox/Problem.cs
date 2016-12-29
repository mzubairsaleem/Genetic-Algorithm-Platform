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
using Open;
using Open.Arithmetic;
using Open.Collections;
using Open.Threading;
using Fitness = GeneticAlgorithmPlatform.Fitness;

namespace AlgebraBlackBox
{

	public delegate double Formula(params double[] p);

	///<summary>
	/// The 'Problem' class is important for tracking fitness results and deciding how well a genome is peforming.
	/// It's possible to have multiple 'problems' being measured at once so each Problem class has to keep a rank of the genomes.
	///</summary>
	public class Problem : GeneticAlgorithmPlatform.IProblem<Genome>
	{

		SortedList<Fitness, Genome> _rankedPool;
		
		// We use a lazy to ensure 100% concurrency since ConcurrentDictionary is optimistic;
		ConcurrentDictionary<string, Lazy<Fitness>> _fitness;
		ConcurrentDictionary<string, Genome> _convergent;
		ConcurrentDictionary<string, bool> _junkYard;

		Formula _actualFormula;


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

		// public Task<double> Correlation(
		// 	double[] aSample, double[] bSample,
		// 	Genome gA, Genome gB)
		// {
		// 	return Task.Run(() =>
		// 	{
		// 		var len = aSample.Length * bSample.Length;

		// 		var gA_result = new double[len];
		// 		var gB_result = new double[len];
		// 		var i = 0;

		// 		foreach (var a in aSample)
		// 		{
		// 			foreach (var b in bSample)
		// 			{
		// 				var p = new double[] { a, b }; // Could be using Tuples?
		// 				var r1 = gA.Calculate(p);
		// 				var r2 = gB.Calculate(p);
		// 				Task.WaitAll(r1, r2);
		// 				gA_result[i] = r1.Result;
		// 				gB_result[i] = r2.Result;
		// 				i++;
		// 			}
		// 		}

		// 		return gA_result.Correlation(gB_result);
		// 	});
		// }


		// // compare(a:AlgebraGenome, b:AlgebraGenome):boolean
		// // {
		// // 	return this.correlation(this.sample(), this.sample(), a, b)>0.9999999;
		// // }


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

		public Genome TakeNextTop()
		{
			Genome result = null;
			ThreadSafety.SynchronizeReadWrite(_rankedPool,
				() => _rankedPool.Any(),
				() =>
				{
					var first = _rankedPool.First();
					_rankedPool.Remove(first.Key);
					result = first.Value;
				});
			return result;
		}

		// async Task GetOwnership(Genome g, Fitness fitness)
		// {
		// 	// Since the Fitness is being worked on, be sure to pull it off the heap.
		// 	Genome rankedG = null;
		// 	while (rankedG == null)
		// 	{
		// 		if (_rankedPool.TryRemoveSynchronized(fitness, out rankedG))
		// 		{
		// 			if (rankedG != g)
		// 			{
		// 				Debug.Fail("Duplicate Genomes!");
		// 			}
		// 		}
		// 		else
		// 		{
		// 			await Task.Yield();
		// 		}
		// 	}
		// }

		// public async Task<T> TakeOwnership<T>(Genome g, Task<T> closure)
		// {
		// 	var fitness = GetFitnessFor(g);
		// 	T result;
		// 	try
		// 	{
		// 		await GetOwnership(g, fitness);
		// 		result = await closure;//.ConfigureAwait(false);
		// 	}
		// 	finally
		// 	{
		// 		if (!_rankedPool.TryAddSynchronized(fitness, g))
		// 		{
		// 			Debug.Fail("Unable to re-add " + g + " into ranked pool.");
		// 		}
		// 		else
		// 		{
		// 			// Debug.WriteLine("Re-added " + g + " back into pool.");
		// 		}
		// 	}
		// 	return result;
		// }

		// public async Task TakeOwnership(Genome g, Task closure)
		// {
		// 	var fitness = GetFitnessFor(g);
		// 	try
		// 	{
		// 		await GetOwnership(g, fitness);
		// 		await closure.ConfigureAwait(false);
		// 	}
		// 	finally
		// 	{
		// 		if (!_rankedPool.TryAddSynchronized(fitness, g))
		// 		{
		// 			Debug.Fail("Unable to re-add gene into ranked pool.");
		// 		} else {
		// 			Debug.WriteLine("Re-added "+g+" back into pool.");
		// 		}
		// 	}
		// }


		Task ProcessTestAsync(IEnumerable<Genome> genomes)
		{
			return Task.WhenAll(
				TestPrep((correct, samples) =>
					genomes.Select(g => ProcessTestAsync(g, correct, samples))
			).ToArray());
		}

		async Task<Fitness> ProcessTestAsync(Genome g, List<double> correct, List<double[]> samples)
		{
			var fitness = GetFitnessFor(g);
			var len = correct.Count;
			var divergence = new double[correct.Count];
			var calc = new double[correct.Count];
			var NaNcount = 0;

			// #if DEBUG
			// 			var gRed = g.AsReduced();
			// #endif

			for (var i = 0; i < len; i++)
			{
				var result = await g.CalculateAsync(samples[i]);
				// #if DEBUG
				// 				if (gRed != g)
				// 				{
				// 					var s = samples[i];
				// 					var rr = await gRed.Calculate(s);
				// 					if (!g.Genes.OfType<ParameterGene>().Any(gg=>gg.ID>1) // For debugging/testing IDs greater than 1 are invalid so ignore.
				// 						&& !result.IsRelativeNearEqual(rr, 1E-7))
				// 					{
				// 						var message = String.Format(
				// 	@"Reduction calculation doesn't match!!! {0} => {1}
				// 	Sample: {2}
				// 	result: {3} != {4}", g, gRed, s.JoinToString(", "), result, rr);
				// 						if (!result.IsNaN())
				// 							Debug.Fail(message);
				// 						else
				// 							Debug.WriteLine(message);
				// 					}
				// 				}
				// #endif
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
				return fitness;
			}

			var c = correct.Correlation(calc);
			var d = divergence.Average() + 1;

			fitness.AddScores(
				(double.IsNaN(c) || double.IsInfinity(c)) ? -2 : c,
				(double.IsNaN(d) || double.IsInfinity(d)) ? double.NegativeInfinity : d
			);

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
					Parallel.ForEach(genomes, g => ProcessTest(g, correct, samples));
					return true;
				});
		}

		Fitness ProcessTest(Genome g, List<double> correct, List<double[]> samples)
		{
			var fitness = GetFitnessFor(g);

			var len = correct.Count;
			var divergence = new double[correct.Count];
			var calc = new double[correct.Count];
			var NaNcount = 0;

			// #if DEBUG
			// 			var gRed = g.AsReduced();
			// #endif

			for (var i = 0; i < len; i++)
			{
				var result = g.Calculate(samples[i]);
				// #if DEBUG
				// 				if (gRed != g)
				// 				{
				// 					var s = samples[i];
				// 					var rr = await gRed.Calculate(s);
				// 					if (!g.Genes.OfType<ParameterGene>().Any(gg=>gg.ID>1) // For debugging/testing IDs greater than 1 are invalid so ignore.
				// 						&& !result.IsRelativeNearEqual(rr, 1E-7))
				// 					{
				// 						var message = String.Format(
				// 	@"Reduction calculation doesn't match!!! {0} => {1}
				// 	Sample: {2}
				// 	result: {3} != {4}", g, gRed, s.JoinToString(", "), result, rr);
				// 						if (!result.IsNaN())
				// 							Debug.Fail(message);
				// 						else
				// 							Debug.WriteLine(message);
				// 					}
				// 				}
				// #endif
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
				return fitness;
			}

			var c = correct.Correlation(calc);
			var d = divergence.Average() + 1;

			fitness.AddScores(
				(double.IsNaN(c) || double.IsInfinity(c)) ? -2 : c,
				(double.IsNaN(d) || double.IsInfinity(d)) ? double.NegativeInfinity : d
			);

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

		// For the most part is not as performant when low sample count and short calculation times.
		// But keep this here for reference.
		// Task<Fitness> ParallelTest(Genome g, List<double> correct, List<double[]> samples)
		// {
		// 	var len = correct.Count;
		// 	var divergence = new double[correct.Count];
		// 	var calc = new double[correct.Count];

		// 	return Task

		// 		.WhenAll(
		// 			Enumerable.Range(0, len)
		// 			.Select(i =>
		// 				g.Calculate(samples[i])
		// 					.ContinueWith(task =>
		// 					{
		// 						var result = task.Result;
		// 						calc[i] = result;
		// 						divergence[i] = -Math.Abs(result - correct[i]);
		// 					}))
		// 			.ToArray())

		// 		.ContinueWith(task =>
		// 		{
		// 			var c = correct.Correlation(calc);
		// 			var d = divergence.Average() + 1;

		// 			var fitness = GetFitnessFor(g);
		// 			fitness.AddScores(
		// 				(double.IsNaN(c) || double.IsInfinity(c)) ? -2 : c,
		// 				(double.IsNaN(d) || double.IsInfinity(d)) ? double.NegativeInfinity : d
		// 			);

		// 			Task.Run(() =>
		// 			{
		// 				var key = g
		// 					.AsReduced()
		// 					.ToString();
		// 				if (fitness.HasConverged())
		// 				{
		// 					_convergent[key] = g;
		// 				}
		// 				else
		// 				{
		// 					Genome v;
		// 					_convergent.TryRemove(key, out v);
		// 				}
		// 			});

		// 			return fitness;
		// 		});
		// }



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

#if DEBUG
			if (correct.All(v => double.IsNaN(v)))
				throw new Exception("Formula cannot render allways NaN.");
#endif

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

	}



}