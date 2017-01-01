using System;
using Open.Collections;
using System.Diagnostics;
using Open;

namespace GeneticAlgorithmPlatform
{
	public class Program
	{
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
		public static void Main(string[] args)
		{
			var sw = Stopwatch.StartNew();
			var e = new AlgebraBlackBox.Environment(SqrtA2B2AB);
			e.ListenToTopChanges(change =>
			{
				var gf = change.Item2;
				Console.WriteLine("{0}:\t{1}", change.Item1.ID, gf.Genome.ToAlphaParameters());
				Console.WriteLine("  \t[{0}] ({1} samples)", gf.Fitness.Scores.JoinToString(","), gf.Fitness.SampleCount);
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
