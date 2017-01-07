using System;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Open;
using Open.Collections;

namespace GeneticAlgorithmPlatform
{
    public class Program
	{
		static double AB(params double[] p)
		{
			var a = p[0];
			var b = p[1];
			return a * b;
		}

		static double SqrtA2B2(params double[] p)
		{
			var a = p[0];
			var b = p[1];
			return Math.Sqrt(a * a + b * b);
		}

		static double SqrtA2B2AB(params double[] p)
		{
			var a = p[0];
			var b = p[1];
			return Math.Sqrt(a * a + b * b + a) + b;
		}
		static readonly double[] OneOne = new double[] { 1, 1 };
		public static void Main(string[] args)
		{
			var sw = Stopwatch.StartNew();
			var env = new AlgebraBlackBox.Environment(AB, 10);
			var prob = ((AlgebraBlackBox.Problem)(env.Problem));

			env.TopGenome.LinkTo(new ActionBlock<AlgebraBlackBox.Genome>(genome =>
			{
				var tc = prob.TestCount;
				Console.WriteLine("{0}:\t{1} => {2}", 1, genome.ToAlphaParameters(), genome.AsReduced().ToAlphaParameters());
				Console.WriteLine("  \t= {0}", genome.Calculate(OneOne));
				var fitness = prob.GetOrCreateFitnessFor(genome).Fitness;
				Console.WriteLine("  \t[{0}] ({1} samples)", fitness.Scores.JoinToString(","), fitness.SampleCount);
				Console.WriteLine("  \t{0} tests, {1} total time, {2} ticks average", tc, sw.Elapsed.ToStringVerbose(), sw.ElapsedTicks / tc);
				Console.WriteLine();
			}));

			env.TopGenome.Completion.Wait();


		}

		static void PerfTest()
		{
			var sw = new Stopwatch();
			sw.Start();

			var n = 0;
			for (var j = 0; j < 10; j++)
			{
				for (var i = 0; i < 1000000000; i++)
				{
					n += i;
				}
			}

			sw.Stop();
			Console.WriteLine("Result: " + n);

			Console.WriteLine("Elapsed Time: " + sw.ElapsedMilliseconds);
		}
	}
}
