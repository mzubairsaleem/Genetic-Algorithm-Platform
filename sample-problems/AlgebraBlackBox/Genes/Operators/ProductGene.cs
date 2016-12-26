using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Open.Arithmetic;

namespace AlgebraBlackBox.Genes
{
	public class ProductGene : OperatorGeneBase
	{
		public const char Symbol = '*';

		public ProductGene(double multiple = 1, IEnumerable<IGene> children = null) : base(Symbol, multiple, children)
		{
		}


		protected async override Task<double> CalculateWithoutMultiple(double[] values)
		{
			// Allow for special case which will get cleaned up.
            var children = GetChildren();
			if (children.Count == 0) return 1;
			var results = await Task.WhenAll(children.Select(s => s.Calculate(values)));
			return results.Product();
		}

		public new ProductGene Clone()
		{
            return new ProductGene(Multiple, GetChildren().Select(g => g.Clone()));	
		}

		protected override GeneticAlgorithmPlatform.IGene CloneInternal()
		{
			return this.Clone();
		}
		void MigrateMultiples()
		{
            var children = GetChildren();
			if (children.Any(c => c.Multiple == 0))
			{
				// Any multiple of zero? Reset the entire collection;
				Clear();
				Multiple = 0;
			}
			else
			{
				// Extract any multiples so we don't have to worry about them later.
				foreach (var c in children.OfType<ConstantGene>().ToArray())
				{
					var m = c.Multiple;
					Remove(c);
					this.Multiple *= m;
				}
				foreach (var c in children)
				{
					var m = c.Multiple;
					if (m != 1d)
					{
						this.Multiple *= m;
						c.Multiple = 1d;
					}
				}
			}
		}

		protected override void ReduceLoop()
		{

			MigrateMultiples();

            var children = GetChildren();
			foreach (var p in children.OfType<DivisionGene>().ToArray())
			{
				Debug.Assert(p.Multiple == 1, "Should have already been pulled out.");
				// Use the internal reduction routine of the division gene to reduce the division node.
				var m = Multiple;
				p.Multiple = m;

				// Dividing by itself?
				var d = p.Children.Where(g => children.Any(a => g != p && g.ToString() == a.ToString()));
				IGene df;
				while ((df = d.FirstOrDefault()) != null)
				{
					p.Remove(df);
					Remove(children.First(g => g.ToString() == df.ToString()));
				}

				if (p.Count == 0)
				{
					Remove(p);
				}
				else
				{
					var pReduced = ChildReduce(p) ?? p;
					Debug.Assert(Multiple == m, "Shouldn't have changed!");
					Multiple = pReduced.Multiple;
					pReduced.Multiple = 1;
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

			// Look for groupings...
			foreach (var p in children
				.GroupBy(g => g.ToStringContents())
				.Where(g => g.Count() > 1))
			{

				// Multiplying more than 1 square root of the same value together?
				var genes = p
					.OfType<SquareRootGene>()
					.ToList();

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

				break;

			}

			// Look for squares that have been rooted.
			foreach (var sr in children
				.OfType<SquareRootGene>()
				.Where(g => g.SingleOrDefault() is ProductGene && g.Count > 1))
			{

				foreach (var p in sr
					.GroupBy(g => g.ToStringContents())
					.Where(g => g.Count() > 1))
				{
					var genes = p.ToList();

					while (genes.Count > 1)
					{
						// Step 1 pull out the extra one.
						var last = genes.Last();
						Debug.Assert(last.Multiple == 1, "Should have already been pulled out.");
						genes.Remove(last);
						sr.Remove(last);

						// Step 2 replace the square root container with the product.
						last = genes.Last();
						Debug.Assert(last.Multiple == 1, "Should have already been pulled out.");
						genes.Remove(last);
						Add(last); // Extract and include in the product.
					}

				}
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