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
			var v = values.Single();
			// Debug.Assert(v != 0);
			return v == 0 ? double.NaN : (1 / v);
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

				if (m != 1 && Multiple % m == 0)
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
			if (children.Count == 1)
			{
				var c = children.Single();
				if (c.Multiple == 0)
				{
					// WHOA.  Divide by zero.
					return new ConstantGene(double.NaN);
				}
				else if (c.Multiple == 1)
				{
					if (c is ConstantGene)
						return new ConstantGene(this.Multiple);
				}

				var d = c as DivisionGene;
				if (d != null && d.Multiple == 1)
				{
					var x = d.Children.Single();
					x.Multiple *= this.Multiple;
					d.Remove(x);
					return x;
				}

			}
			return base.ReplaceWithReduced();
		}

		public override string ToStringContents()
		{
			var children = GetChildren();
			return children.Count == 1 ? children.Single().ToString() : "";
		}

		public override string ToStringUsingMultiple(double m)
		{
			return String.Format("({0}/{1})", m, ToStringContents());
		}

		protected override string ToStringInternal()
		{
			return ToStringUsingMultiple(Multiple);
		}

	}
}