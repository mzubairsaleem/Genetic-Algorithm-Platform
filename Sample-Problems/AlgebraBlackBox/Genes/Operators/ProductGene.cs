using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Open.Arithmetic;
using Open.Collections;

namespace AlgebraBlackBox.Genes
{
	public class ProductGene : OperatorGeneBase
	{
		public const char Symbol = '*';

		public ProductGene(double multiple = 1, IEnumerable<IGene> children = null) : base(Symbol, multiple, children)
		{
		}

		public ProductGene(IEnumerable<IGene> children) : base(Symbol, 1, children)
		{
		}

		protected override double DefaultIfNoChildren()
		{
			return 1d; // This means 'not divided by anything'.
		}

		protected override double ProcessChildValues(IEnumerable<double> values)
		{
			return values.Product();
		}

		ProductGene CloneThis()
		{
			return new ProductGene(Multiple, CloneChildren());
		}
		public new ProductGene Clone()
		{
			return CloneThis();
		}

		protected override GeneticAlgorithmPlatform.IGene CloneInternal()
		{
			return CloneThis();
		}

		bool MigrateMultiple(IGene child)
		{
			var m = child.Multiple;
			if (m != 1 && double.IsInfinity(m))
			{
				this.Multiple *= m;
				child.Multiple = 1d;
			}
			return false;
		}

		bool MigrateMultiples()
		{
			var children = GetChildren();
			if (children.Any(c => double.IsNaN(c.Multiple)))
			{
				// Any multiple of NaN? Reset the entire collection;
				Clear();
				Multiple = double.NaN;
				return true;
			}

			if (children.Any(c => c.Multiple == 0))
			{
				// Any multiple of zero? Reset the entire collection;
				Clear();
				Multiple = 0;
				return true;
			}

			var updated = false;
			// Extract any multiples so we don't have to worry about them later.
			foreach (var c in children)
			{
				if (MigrateMultiple(c)) updated = true;
				if (c is ConstantGene)
				{
					Remove(c); // With multples neutralized, then constants are superflous.
					updated = true;
				}
			}

			if (System.Math.Abs(this.Multiple) > 1)
			{
				foreach (var d in children.OfType<DivisionGene>())
				{
					var c = d.Children.FirstOrDefault();
					if (c != null)
					{
						var m = c.Multiple;
						if (double.IsNaN(m) || double.IsInfinity(m))
						{
							// Can't handle this so move on.
							continue;
						}

						// First try to divide by entire multiple.
						if (this.Multiple % m == 0)
						{
							this.Multiple /= m;
							c.Multiple /= m;
							updated = true;
						}
						else
						{
							foreach (var i in m.DivisibleMultiples().Distinct())
							{
								while (this.Multiple % i == 0)
								{
									this.Multiple /= i;
									c.Multiple /= i;
									updated = true;
								}

								if (System.Math.Abs(this.Multiple) == 1)
									break;
							}
						}

						if (System.Math.Abs(this.Multiple) == 1)
							break;
					}
				}
			}

			return updated;
		}

		protected override void ReduceLoop()
		{
			if (Count == 0)
				return;

			if (Multiple == 0)
			{
				Clear();
				return;
			}

			if (MigrateMultiples())
				return;

			var children = GetChildren();
			foreach (var d in children.OfType<DivisionGene>().ToArray())
			{
				Debug.Assert(d.Multiple == 1, "Should have already been pulled out.");
				// Use the internal reduction routine of the division gene to reduce the division node.
				var m = Multiple;

				// Dividing by itself?
				var c = d.Children.Where(
					g => children.Any(
						a => g != d && g.ToStringUsingMultiple(1) == a.ToStringUsingMultiple(1)));
				IGene df;
				while ((df = c.FirstOrDefault()) != null)
				{
					var n = children.First(g => g != d && g.ToStringUsingMultiple(1) == df.ToStringUsingMultiple(1));
					Debug.Assert(n.Multiple == 1, "Should have already been pulled out.");
					Remove(n);

					if (df.Multiple == 1)
						d.Remove(df);
					else
						d.ReplaceChild(df, new ConstantGene(df.Multiple));
				}

				if (d.Count == 0)
				{
					ReplaceChild(d, new ConstantGene(d.Multiple));
				}
				else
				{
					var pReduced = ChildReduce(d) ?? d;
					Debug.Assert(Multiple == m, "Shouldn't have changed!");
					if (pReduced != d && pReduced.Multiple != 1)
					{
						Multiple *= pReduced.Multiple;
						pReduced.Multiple = 1;
					}
				}

			}

			// Collapse products within products.
			foreach (var p in children.OfType<ProductGene>().ToArray())
			{
				Debug.Assert(p.Multiple == 1, "Should have already been pulled out.");
				children.AddThese(p.Children);
				p.Clear();
				children.Remove(p);
			}

			if (MigrateMultiples()) return;

			// Look for square root groupings...
			foreach (var p in children
				.OfType<SquareRootGene>()
				.GroupBy(g => g.ToStringUsingMultiple(1))
				.Where(g => g.Count() > 1))
			{

				// Multiplying more than 1 square root of the same value together?
				var genes = p.ToList();

				while (genes.Count > 1)
				{
					// Step 1 pull out the extra one.
					var last = genes.Last();
					Debug.Assert(last.Multiple == 1, "Should have already been pulled out.");
					genes.Remove(last);
					Remove(last);

					// Step 2 replace the square root container with the product.
					last = genes.Last();
					Debug.Assert(last.Multiple == 1, "Should have already been pulled out.");
					genes.Remove(last);
					ReplaceChild(last, last.Single()); // It should only be single.. If not, we have a serious problem somewhere else.
				}

			}

			// Look for cancellations... (probably could be optimized more)
			foreach (var d in children
				.OfType<DivisionGene>()
				.Where(g => g.HasAny()).ToArray())
			{
				var other = children.Where(c => c != d && c.ToStringUsingMultiple(1) == d.ToStringContents()).FirstOrDefault();
				if (other != null)
				{
					Debug.Assert(d.Multiple == 1, "Should have already been pulled out.");
					Debug.Assert(other.Multiple == 1, "Should have already been pulled out.");
					Debug.Assert(d.Count == 1);
					d.Clear();
					Remove(d);
					Remove(other);
				}
				else
				{
					var p = d.Children.Single() as ProductGene;
					if (p != null)
					{
						foreach (var e in p.Children.ToArray())
						{
							if (e.Multiple != 1)
							{
								p.Multiple *= e.Multiple;
								e.Multiple = 1;
							}
							var o2 = children.Where(c => c.ToStringUsingMultiple(1) == e.ToStringUsingMultiple(1)).FirstOrDefault();
							if (o2 != null)
							{
								p.Remove(e);
								Remove(o2);
							}

						}
					}
				}
			}

			var divisions = children.OfType<DivisionGene>().Where(c => c.HasAny()).ToArray();
			if (divisions.Length > 1)
			{
				var newProd = new ProductGene();
				foreach (var d in divisions)
				{
					Remove(d);
					newProd.Multiple *= d.Multiple;
					newProd.Add(d.Single());
					d.Clear();
				}
				Add(new DivisionGene(newProd));
			}
		}

		protected override IGene ReplaceWithReduced()
		{
			var children = GetChildren();
			if (children.Count == 1)
			{
				var c = children.Single();
				c.Multiple *= this.Multiple;
				return c;
			}
			return base.ReplaceWithReduced();
		}

	}
}