using System;
using System.Collections.Generic;
using System.Linq;
using Open.Arithmetic;

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
			foreach (var c in children.ToArray())
			{
				var m = c.Multiple;

				// Pull out negatives first.
				if (m < 0)
				{
					m *= -1;
					c.Multiple = m;
					this.Multiple *= -1;
				}

				if (m==0 || double.IsNaN(m))
				{
					this.Multiple = double.NaN;
					Clear();
					return;
				}

				// There are edge cases where infinity * 0 = 0 so don't mess with this.
				if (double.IsInfinity(m))
				{
					continue;
				}

				// Find workable multiples and cancel them.
				foreach (var i in m.Multiples().Skip(1).Distinct())
				{
					while (this.Multiple % i == 0)
					{
						m /= i;
						this.Multiple /= i;
						c.Multiple = m;
					}

					if (System.Math.Abs(this.Multiple) == 1)
						break;
				}

				if (c.Multiple == 1 && c is ConstantGene)
				{
					children.Remove(c);
				}
			}
		}

		protected override IGene ReplaceWithReduced()
		{
			var children = GetChildren();
			if (children.Count == 1)
			{
				var c = children.Single();
				var m = c.Multiple;
				// m==0 and m==NaN is handled above in ReduceLoop.
				if (m == 1)
				{
					if (c is ConstantGene)
						return new ConstantGene(this.Multiple);
				}

				// Dividing?  Just replace this with it.
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