using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.Arithmetic;
using Open.Collections;

namespace AlgebraBlackBox.Genes
{
	public class SumGene : OperatorGeneBase
	{
		public const char Symbol = '+';

		public SumGene(double multiple = 1, IEnumerable<IGene> children = null) : base(Symbol, multiple, children)
		{
		}

		protected async override Task<double> CalculateWithoutMultipleAsync(double[] values)
		{
			using (var results =
                GetChildren()
                .Select(s => s.CalculateAsync(values)).Memoize())
			{
				return results.Any() ? (await Task.WhenAll(results).ConfigureAwait(false)).Sum() : 0;
			}
		}

		protected override double CalculateWithoutMultiple(double[] values)
		{
			using (var results =
                GetChildren()
                .Select(s => s.Calculate(values)).Memoize())
			{
				return results.Any() ? results.Sum() : 0;
			}
		}

		SumGene CloneThis()
		{
			return new SumGene(Multiple, CloneChildren());
		}

		public new SumGene Clone()
		{
			return CloneThis();
		}

		protected override GeneticAlgorithmPlatform.IGene CloneInternal()
		{
			return CloneThis();
		}

		protected override string ToStringInternal()
		{
			return base.ToStringInternal().Replace("+-", "-");
		}

		void RemoveZeroMultiples()
		{
			foreach (var g in GetChildren().Where(g => g.Multiple == 0).ToArray())
			{
				Remove(g);
			}
		}

		protected override void ReduceLoop()
		{
			// Collapse sums within sums.
			var children = GetChildren();
			foreach (var p in children.OfType<SumGene>().ToArray())
			{
				var m = p.Multiple;
				foreach (var s in p)
				{
					s.Multiple *= m;
					children.Add(s);
				}
				p.Clear();
				children.Remove(p);
			}

			// Pull out multiples.
			if (children.Any() && children.All(g => Math.Abs(g.Multiple) > 1 || g.Multiple == -1))
			{
				var smallest = children.OrderBy(g => g.Multiple).First();
				var max = smallest.Multiple;
				for (var i = 2; i <= max; i = i.NextPrime())
				{
					while (max % i == 0 && children.All(g => g.Multiple % i == 0))
					{
						max /= i;
						Multiple *= i;
						foreach (var g in children)
						{
							g.Multiple /= i;
						}
					}
				}

			}

			// Combine any constants.  This is more optimal because we don't neet to query ToStringContents.
			var constants = children.OfType<ConstantGene>().ToArray();
			if (constants.Length > 1)
			{
				var main = constants.First();
				foreach (var c in constants.Skip(1))
				{
					main.Multiple += c.Multiple;
					children.Remove(c);
				}
			}

			RemoveZeroMultiples();

			// Look for groupings...
			foreach (var p in children
				.Where(g => !(g is ConstantGene)) // We just reduced constants above so skip them...
				.GroupBy(g => g.ToStringContents())
				.Where(g => g.Count() > 1))
			{
				using (var matches = p.Memoize())
				{
					// Take matching groupings and merge them.
					var main = matches.First();
					var sum = matches.Sum(s => s.Multiple);

					if (sum == 0)
						// Remove the gene that would remain with a zero.
						Remove(main);
					else
						main.Multiple = sum;

					// Remove the other genes that are now useless.
					foreach (var gene in matches.Skip(1))
						Remove(gene);

					break;
				}
			}

			RemoveZeroMultiples();
		}

		protected override IGene ReplaceWithReduced()
		{
			var children = GetChildren();
			switch (children.Count)
			{
				case 0:
					return new ConstantGene(0);
				case 1:
					var c = children.Single();
					c.Multiple *= Multiple;
					return c;

			}
			return base.ReplaceWithReduced();
		}


	}
}
