using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Open.Math;

namespace Sandbox
{
	class Problem
	{
		public GeneFactory GetGeneFactory()
		{
			return new GeneFactory(2);
		}

		public double Correlation(double[] aSample, double[] bSample, Genome gA, Genome gB)
		{
			var len = aSample.Length * bSample.Length;

			var gAresult = new List<double>(len);
			var gBresult = new List<double>(len);
			foreach (var a in aSample)
			{
				foreach (var b in bSample)
				{
					gAresult.Add(gA.Root.Calculate(new double[2] { a, b }));
					gBresult.Add(gB.Root.Calculate(new double[2] { a, b }));
				}
			}

			return gAresult.Correlation(gBresult);

		}


		public bool Compare(Genome a, Genome b, int testcount = 1)
		{
			return Correlation(Sample(), Sample(), a, b) > (1 - double.Epsilon);
		}

		public double[] Sample(double count = 5, double range = 100)
		{
			var result = new HashSet<double>();
			while(result.Count<count)
				result.Add(Environment.Randomizer.NextDouble() * range);
			return result.OrderBy(v=>v).ToArray();
		}


		public void Test(Population p, int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				var aSample = Sample();
				var bSample = Sample();
				var len = aSample.Length * bSample.Length;
				var correct = new List<double>(len);

				foreach (var a in aSample)
				{
					foreach (var b in bSample)
					{
						correct.Add(ActualFormula(a, b));
					}
				}


				foreach (var o in p)
				{
					var result = new List<double>(len);
					foreach (var a in aSample)
					{
						foreach (var b in bSample)
						{
							result.Add(o.Genome.Root.Calculate(new double[2] { a, b }));
						}
					}

					o.AddFitness(correct.Correlation(result));
				}

			}
		}

		

		private static double ActualFormula(double a, double b) // Solve for 'c'.
		{
			return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
		}


	}
}
