/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeneticAlgorithmPlatform;
using Open.Arithmetic;
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
		public readonly SampleCache Samples;

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
			Samples = new SampleCache(actualFormula);
		}

		protected override Genome GetFitnessForKeyTransform(Genome genome)
		{
			return genome.AsReduced();
		}

		protected override async Task<IFitness> ProcessTest(Genome g, long sampleId)
		{
			var f = new Fitness();
			await ProcessTest(g, f, sampleId, true);
			Interlocked.Increment(ref _testCount);
			return f;
		}

		protected async Task ProcessTest(Genome g, Fitness fitness, long sampleId, bool useAsync = true)
		{
			var samples = Samples.Get(sampleId);
			var len = samples.Length;
			var correct = new double[len];
			var divergence = new double[len];
			var calc = new double[len];
			var NaNcount = 0;

// #if DEBUG
// 			var gRed = g.AsReduced();
// #endif

			for (var i = 0; i < len; i++)
			{
				var sample = samples[i];
				var s = sample.Key;
				var correctValue = sample.Value;
				correct[i] = correctValue;
				var result = useAsync ? await g.CalculateAsync(s) : g.Calculate(s);
// #if DEBUG
// 				if (gRed != g)
// 				{
// 					var rr = useAsync ? await gRed.CalculateAsync(s) : gRed.Calculate(s);
// 					if (!g.Genes.OfType<ParameterGene>().Any(gg => gg.ID > 1) // For debugging/testing IDs greater than 1 are invalid so ignore.
// 						&& !result.IsRelativeNearEqual(rr, 7))
// 					{
// 						var message = String.Format(
// 							"Reduction calculation doesn't match!!! {0} => {1}\n\tSample: {2}\n\tresult: {3} != {4}",
// 							g, gRed, s.JoinToString(", "), result, rr);
// 						if (!result.IsNaN())
// 							Debug.WriteLine(message);
// 						else
// 							Debug.WriteLine(message);
// 					}
// 				}
// #endif
				if (!double.IsNaN(correctValue) && double.IsNaN(result)) NaNcount++;
				calc[i] = result;
				divergence[i] = -Math.Abs(result - correctValue);
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
				var c = correct.Correlation(calc.Where(v => !double.IsNaN(v)));
				var d = divergence.Where(v => !double.IsNaN(v)).Average() + 1;

				fitness.AddScores(
					(double.IsNaN(c) || double.IsInfinity(c)) ? -2 : c,
					(double.IsNaN(d) || double.IsInfinity(d)) ? double.NegativeInfinity : d
				);
			}
		}



	}



}