﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using GeneticAlgorithmPlatform.Schemes;
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

		static double SqrtA2B2A2B1(params double[] p)
		{
			var a = p[0];
			var b = p[1];
			return Math.Sqrt(a * a + b * b + a + 2) + b + 1;
		}
		static readonly double[] OneOne = new double[] { 1, 1 };
		public static void Main(string[] args)
		{
			Console.WriteLine("Starting...");
			// Througput test...
			// var factory = new AlgebraBlackBox.GenomeFactory();
			// factory.Generate().AsParallel().ForAll(g=>{

			// });

			// return; 
			var problem = new AlgebraBlackBox.Problem(SqrtA2B2);
			var scheme = new PyramidPipeline<AlgebraBlackBox.Genome>(
				new AlgebraBlackBox.GenomeFactory(),
				20, 3, 2);
			
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

			Action<AlgebraBlackBox.Genome> emitGenomeStats = genome =>
			{
				var fitness = problem.GetOrCreateFitnessFor(genome).Fitness;

				var asReduced = genome.AsReduced();
				if (asReduced == genome)
					Console.WriteLine("{0}:\t{1}", 1, genome.ToAlphaParameters());
				else
					Console.WriteLine("{0}:\t{1}\n=>\t{2}", 1, genome.ToAlphaParameters(), asReduced.ToAlphaParameters());

				Console.WriteLine("  \t(1,1) = {0}", genome.Calculate(OneOne));
				Console.WriteLine("  \t[{0}] ({1} samples)", fitness.Scores.JoinToString(","), fitness.SampleCount);
				Console.WriteLine();
			};

			bool done = false;
			scheme.TopGenome.LinkTo(new ActionBlock<AlgebraBlackBox.Genome>(emitGenomeStats));

			Task.Run(async () =>
			{
				while (!done)
				{
					emitStats();

					await Task.Delay(5000);
				}
			});

			sw.Start();
			scheme.Start(problem).ContinueWith(task =>
			{
				done = true;
				emitStats();
				Console.WriteLine();
				if (task.IsFaulted)
					Console.WriteLine(task.Exception.GetBaseException());
				else
					Console.WriteLine("Done.");
			}).Wait();

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
