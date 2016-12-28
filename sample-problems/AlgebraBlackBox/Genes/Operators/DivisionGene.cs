using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.Arithmetic;

namespace AlgebraBlackBox.Genes
{
	public class DivisionGene : OperatorGeneBase
	{
		public const char Symbol = '/';

		public DivisionGene(double multiple = 1, IEnumerable<IGene> children = null) : base(Symbol, multiple, children)
		{
		}

		protected async override Task<double> CalculateWithoutMultipleAsync(double[] values)
		{
			// Allow for special case which will get cleaned up.
			var children = GetChildren();
			if (children.Count == 0) return 1;
            return (await Task.WhenAll(children.Select(s => s.CalculateAsync(values))).ConfigureAwait(false)).QuotientOf(1);
		}

		protected override double CalculateWithoutMultiple(double[] values)
		{
			// Allow for special case which will get cleaned up.
			var children = GetChildren();
			if (children.Count == 0) return 1;
			return children.Select(s => s.Calculate(values)).QuotientOf(1);
		}

		DivisionGene CloneThis()
		{
			return new DivisionGene(Multiple, CloneChildren());
		}

		public new DivisionGene Clone()
		{
			return CloneThis();
		}

		protected override GeneticAlgorithmPlatform.IGene CloneInternal()
		{
			return CloneThis();
		}

		override protected void ReduceLoop()
		{
			// Pull out clean divisors.
			var children = GetChildren();
			foreach (var g in children.ToArray())
			{
				var m = g.Multiple;

				// Pull out negatives first.
				if(m<0)
				{
					m *= -1;
					g.Multiple *= m;
					Multiple *= -1;
				}

				if (Multiple % m == 0)
				{
					g.Multiple = 1;
					if (g is ConstantGene)
						children.Remove(g);
					Multiple /= m;
				}
			}
		}

		protected override IGene ReplaceWithReduced()
		{
			var children = GetChildren();
			var d = (children.Count == 1 ? children.SingleOrDefault() : null) as DivisionGene;
			if (d != null && d.Multiple == 1)
			{
				d.Multiple *= this.Multiple;
				return d;
			}
			return base.ReplaceWithReduced();
		}

		public override string ToStringContents()
		{

			return GroupedString(Operator, Multiple.ToString() + Symbol);
		}

		protected override string ToStringInternal()
		{
			return ToStringContents();
		}

	}
}