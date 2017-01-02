using System;
using Open.Collections;
using System.Diagnostics;
using Open;
using AlgebraBlackBox;

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
			var e = new AlgebraBlackBox.Environment(SqrtA2B2);
			e.ListenToTopChanges(change =>
			{
				var prob = (Problem)change.Item1;
				var tc = prob.TestCount;
				var gf = change.Item2;
				Console.WriteLine("{0}:\t{1}", prob.ID, gf.Genome.ToAlphaParameters());
				Console.WriteLine("  \t= {0}", gf.Genome.Calculate(OneOne));
				Console.WriteLine("  \t[{0}] ({1} samples)", gf.Fitness.Scores.JoinToString(","), gf.Fitness.SampleCount);
				Console.WriteLine("  \t{0} tests, {1} total time, {2} ticks average", tc, sw.Elapsed.ToStringVerbose(), sw.ElapsedTicks / tc);
				Console.WriteLine();
			});


			var converged = e.WaitForConverged();
			e.SpawnNew();
			converged.Wait();

			Console.WriteLine("Converged: (after {0})", sw.Elapsed.ToStringVerbose());
			int i = 0;
			foreach (var problem in converged.Result)
			{
				Console.WriteLine((++i) + ":");
				foreach (var genome in problem.Convergent)
				{
					Console.Write("  \t");
					Console.WriteLine(genome.ToAlphaParameters());
				}
			}
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
