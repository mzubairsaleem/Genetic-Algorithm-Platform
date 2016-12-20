using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	class MainClass
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			var e = new Environment(new Problem());

			var size = 50;
			var testCount = 3;

			// Initial...
			var p = e.Spawn(size);
			e.Problem.Test(p, testCount);

			int generations = 0;
			do
			{
				generations++;
				var orgs = p.OrderBy(o => o.FitnessAverage).ThenBy(o => o.Genome.Genes.Count);
				foreach (var o in orgs)
				{
					var ts = o.Genome.ToString();
					Console.WriteLine(String.Format(ts, "a", "b"));
					var tsr = o.Genome.ToStringReduced();
					if(tsr!=ts)
						Console.WriteLine("Reduced: "+String.Format(tsr, "a", "b"));
					Console.WriteLine("Fitness --> " + o.FitnessAverage);
					Console.WriteLine(String.Empty);
				}
				Console.WriteLine("Generation #" + generations);

				if (p.Any(o => o.FitnessAverage > 1 - double.Epsilon))
				{
					Console.WriteLine("EUREKA!");
					while (true)
						Console.ReadLine();
				}
				else { 

					p.KeepWinners(size / 2);

					Console.WriteLine("Hit the enter key when finished...\n");
					Console.ReadLine();

					p = e.SpawnFrom(e.Populations.SelectMany(o=>o).OrderByDescending(o=>o.FitnessAverage).Take(size), size);

					if(Debugger.IsAttached) foreach(var g in p.Select(o=>o.Genome).Where(o=>o.ToString()!=o.ToStringReduced()))
					{
						var gcr = g.CloneReduced();
						if (!e.Problem.Compare(g, gcr))
						{
							//Debug.Fail("Mismatch", g.ToString() + " != " + gcr.ToString());
						}
						
					}

					e.Problem.Test(p, testCount);
				}

			}
			while (true);
		}
	}
}
