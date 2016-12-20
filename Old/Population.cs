using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	class Population : List<Organism>
	{
		public Population(Problem problem, GeneFactory genefactory)
		{
			Contract.Requires(problem != null);

			Problem = problem;
			GeneFactory = genefactory;
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

		public void Populate(int count = 1)
		{
			for (var i = 0; i < count; i++)
				Add(new Organism(GeneFactory.Generate()));
		}

		public void AddSmart(Organism potential)
		{
			if (potential.Genome != null)
			{
				var ts = potential.Genome.ToString();
				if (!this.Any(o => o.ToString() == ts))// || o.Genome.ToStringReduced() == ts))
					Add(potential);
			}

		}

		public void PopulateFrom(IEnumerable<Organism> source, int count = 1, int transferBest = 0)
		{
			// Be sure to add randomness in...
			AddSmart(new Organism(GeneFactory.Generate()));

			// Then add mutations from best in source.
			for (var i = 0; i < count - transferBest - 1; i++)
				AddSmart(new Organism(GeneFactory.GenerateFrom(source)));

			var enumerator = source.OrderByDescending(o => o.FitnessAverage).Take(transferBest).GetEnumerator();

			// Then transfer the best over to the population so it's core material isn't lost.
			while (Count < count && enumerator.MoveNext())
			{
				var o = enumerator.Current;

				// Include reduced versions...
				if (o.Genome.ToString() != o.Genome.ToStringReduced())
					AddSmart(new Organism(o.Genome.CloneReduced()));

				AddSmart(o);
			}

		}

		public void KeepWinners(int count)
		{
			// Start by clearing invalids.
			foreach (var invalid in this.Where(o =>
			{
				var fa = o.FitnessAverage;
				return double.IsNaN(fa) || double.IsInfinity(fa);
			}).ToArray())
			{
				this.Remove(invalid);
			}

			var orgs = this.OrderBy(o => o.FitnessAverage);

			foreach (var o in orgs)
			{
				if (this.Count <= count)
					break;

				this.Remove(o);
			}
		}

	}
}
