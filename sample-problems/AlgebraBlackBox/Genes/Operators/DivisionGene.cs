using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraBlackBox.Genes
{
    public class DivisionGene : FunctionGene
	{
		public const char Symbol = '/';

		protected DivisionGene(double multiple, IEnumerable<IGene> children) : base(Symbol, multiple, children)
		{
		}

		public DivisionGene(double multiple = 1, IGene child = null) : base(Symbol, multiple, child)
		{
		}

		public DivisionGene(IGene child) : base(Symbol, 1, child)
		{
		}

        protected override double DefaultIfNoChildren()
        {
            return 1d; // This means 'not divided by anything'.
        }

		public override void Add(IGene target)
		{
			if (GetChildren().Count == 1)
				throw new InvalidOperationException("A DivisionGene can only have 1 child.");

			base.Add(target);
		}

        protected override double ProcessChildValues(IEnumerable<double> values)
        {
			return 1 / values.Single();
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
				if (m < 0)
				{
					m *= -1;
					g.Multiple = m;
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
			var d = (children.Count == 1 ? children.Single() : null) as DivisionGene;
			if (d != null && d.Multiple == 1)
			{
				d.Multiple *= this.Multiple;
				return d;
			}
			return base.ReplaceWithReduced();
		}

		public override string ToStringContents()
		{
			var children = GetChildren();
			return children.Count==1 ? children.Single().ToString() : "";
		}

		protected override string ToStringInternal()
		{
			return String.Format("({0}/{1})", Multiple, ToStringContents());
		}

    }
}