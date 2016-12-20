using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraBlackBox
{
	public delegate double Formula(params double[] p);
	class Environment
	{
		Formula _formula;
		public Environment(Formula formula)
		{
			_formula = formula;
		}
		public static Random Randomizer = new Random((int)DateTime.Now.Ticks);

		public static int NextRandomIntegerExcluding(int range, IEnumerable<int> excluded)
		{
			var r = Enumerable.Range(0, range).ToList();
			foreach(var x in excluded)
				r.Remove(x);

			return r[Randomizer.Next(r.Count)];
		}

		public static int NextRandomIntegerExcluding(int range, int excluded)
		{
			var n = Randomizer.Next(range - 1);
			if (n >= excluded)
				n++;
			return n == range ? -1 : n;
		}

		public void TrimEarlyPopulations(int maxPopulations)
		{
			while (Populations.Count > maxPopulations)
				Populations.RemoveAt(0);
		}

		public Environment(Problem problem)
		{
			Problem = problem;
			Populations = new List<Population>();
			GeneFactory = Problem.GetGeneFactory();
		}

		/// <summary>
		/// Runs a test cycle on the current population using the specified problem.
		/// </summary>
		public Population Spawn(int populationSize)
		{
			var p = new Population(Problem, GeneFactory);
			p.Populate(populationSize);
			Populations.Add(p);
			GeneFactory.TrimPreviousGenomes();
			TrimEarlyPopulations(10);
			return p;
		}

		/// <summary>
		/// Runs a test cycle on the current population using the specified problem.
		/// </summary>
		public Population SpawnFrom(IEnumerable<Organism> source, int populationSize)
		{
			var p = new Population(Problem, GeneFactory);
			p.PopulateFrom(source, populationSize, 5);
			Populations.Add(p);
			GeneFactory.TrimPreviousGenomes();
			TrimEarlyPopulations(10);
			return p;
		}

		public GeneFactory GeneFactory
		{
			get;
			private set;
		}

		public Problem Problem
		{
			get;
			private set;
		}

		public List<Population> Populations
		{
			get;
			private set;
		}


	}
}
