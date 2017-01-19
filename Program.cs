﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Open.Dataflow;
using GeneticAlgorithmPlatform.Schemes;
using Open;
using AlgebraBlackBox;
using System.Threading;
using Open.Threading;
using System.Collections.Generic;

namespace GeneticAlgorithmPlatform
{
	public class Program
	{


		static double AB(IReadOnlyList<double> p)
		{
			var a = p[0];
			var b = p[1];
			return a * b;
		}

		static double SqrtA2B2(IReadOnlyList<double> p)
		{
			var a = p[0];
			var b = p[1];
			return Math.Sqrt(a * a + b * b);
		}

		static double SqrtA2B2C2(IReadOnlyList<double> p)
		{
			var a = p[0];
			var b = p[1];
			var c = p[2];
			return Math.Sqrt(a * a + b * b + c * c);
		}

		static double SqrtA2B2A2B1(IReadOnlyList<double> p)
		{
			var a = p[0];
			var b = p[1];
			return Math.Sqrt(a * a + b * b + a + 2) + b + 1;
		}
		public static void Main(string[] args)
		{
			Console.WriteLine("Starting...");

			var problem = new Problem(SqrtA2B2);
			var scheme = new PyramidPipeline<AlgebraBlackBox.Genome>(
				new AlgebraBlackBox.GenomeFactory(),
				50, 5, 3);
			scheme.AddProblem(problem);

			var cancel = new CancellationTokenSource();
			var sw = new Stopwatch();
			Action emitStats = () =>
			{
				var tc = problem.TestCount;
				if (tc != 0)
				{
					Console.WriteLine("{0} tests, {1} total time, {2} ticks average", tc, sw.Elapsed.ToStringVerbose(), sw.ElapsedTicks / tc);
					Console.WriteLine();
				}
			};

			scheme
				.AsObservable()
				.Subscribe(
					Problem.EmitTopGenomeStats,
					ex => Console.WriteLine(ex.GetBaseException()),
					() =>
					{
						cancel.Cancel();
						emitStats();
					});

			Task.Run(async () =>
			{
				while (!cancel.IsCancellationRequested)
				{
					await Task.Delay(5000, cancel.Token);
					emitStats();
				}


			}, cancel.Token);

			sw.Start();
			scheme
				.Start()
				.OnFullfilled(() => Console.WriteLine("Done."))
				.OnFaulted(ex => { throw ex; })
				.Wait();

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

