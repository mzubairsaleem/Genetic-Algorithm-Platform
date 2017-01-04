using System;
using System.Diagnostics;
using Open;
using AlgebraBlackBox;
using System.Threading.Tasks.Dataflow;

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
			var problem = new AlgebraBlackBox.Problem(SqrtA2B2AB);
			var factory = new AlgebraBlackBox.GenomeFactory();
			var network = GenomeSelector<Genome>.BuildNetwork(50, 8, problem.ProcessTest);
			factory.LinkReception(network.Item1);

			network.Item2.LinkTo(new ActionBlock<Genome>(genome =>
			{
				var tc = problem.TestCount;
				Console.WriteLine("{0}:\t{1}", problem.ID, genome.ToAlphaParameters());
				//Console.WriteLine("  \t= {0}", gf.Genome.Calculate(OneOne));
				// Console.WriteLine("  \t[{0}] ({1} samples)", gf.Fitness.Scores.JoinToString(","), gf.Fitness.SampleCount);
				Console.WriteLine("  \t{0} tests, {1} total time, {2} ticks average", tc, sw.Elapsed.ToStringVerbose(), sw.ElapsedTicks / tc);
				Console.WriteLine();
			}));

			factory.Generate(100);

			network.Item2.Completion.Wait();


			// var converged = e.WaitForConverged();
			// e.SpawnNew();
			// converged.Wait();

			// Console.WriteLine("Converged: (after {0})", sw.Elapsed.ToStringVerbose());
			// int i = 0;
			// foreach (var problem in converged.Result)
			// {
			// 	Console.WriteLine((++i) + ":");
			// 	foreach (var genome in problem.Convergent)
			// 	{
			// 		Console.Write("  \t");
			// 		Console.WriteLine(genome.ToAlphaParameters());
			// 	}
			// }
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
