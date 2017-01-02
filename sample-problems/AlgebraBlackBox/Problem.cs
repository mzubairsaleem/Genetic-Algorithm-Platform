/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlgebraBlackBox.Genes;
using Open.Arithmetic;
using Open.Collections;
using Open.Formatting;
using Fitness = GeneticAlgorithmPlatform.Fitness;

namespace AlgebraBlackBox
{

	public delegate double Formula(params double[] p);

	///<summary>
	/// The 'Problem' class is important for tracking fitness results and deciding how well a genome is peforming.
	/// It's possible to have multiple 'problems' being measured at once so each Problem class has to keep a rank of the genomes.
	///</summary>
	public class Problem : GeneticAlgorithmPlatform.ProblemBase<Genome>
	{
		SampleCache _sampleCache;

		long _testCount = 0;
		public long TestCount
		{
			get
			{
				return _testCount;

			}
		}

		public Problem(Formula actualFormula)
		{
			_sampleCache = new SampleCache(actualFormula);
		}

		protected override Genome GetFitnessForKeyTransform(Genome genome)
		{
			return genome.AsReduced();
		}

		protected override async Task ProcessTest(GeneticAlgorithmPlatform.GenomeFitness<Genome, Fitness> gf, bool useAsync = true)
		{
			var fitness = gf.Fitness;
			var g = gf.Genome;
			var samples = _sampleCache.Get(fitness.SampleCount);
			var len = samples.Length;
			var correct = new double[len];
			var divergence = new double[len];
			var calc = new double[len];
			var NaNcount = 0;

#if DEBUG
			var gRed = g.AsReduced();
#endif

			for (var i = 0; i < len; i++)
			{
				var sample = samples[i];
				var s = sample.Key;
				correct[i] = sample.Value;
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
			}
			else
			{
				var c = correct.Correlation(calc);
				var d = divergence.Average() + 1;

				fitness.AddScores(
					(double.IsNaN(c) || double.IsInfinity(c)) ? -2 : c,
					(double.IsNaN(d) || double.IsInfinity(d)) ? double.NegativeInfinity : d
				);
			}
			Interlocked.Increment(ref _testCount);
		}

		protected override List<Genome> RejectBadAndThenReturnKeepers(
			IEnumerable<GeneticAlgorithmPlatform.IGenomeFitness<Genome, Fitness>> source,
			out List<Fitness> rejected)
		{
			var keep = new List<Genome>();
			rejected = new List<Fitness>();

			foreach (var genome in source)
			{
				var scores = genome.Fitness.Scores;
				if (scores.Any(d => double.IsNegativeInfinity(d)) || scores[0] < 0.9 && genome.Fitness.SampleCount > 20)
				{
					Rejected.TryAdd(genome.Genome.Hash, true);
					rejected.Add(genome.Fitness);
				}
				else
				{
					keep.Add(genome.Genome);
				}
			}

			return keep;
		}
	}



}