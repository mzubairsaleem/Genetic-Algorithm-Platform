using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AlgebraBlackBox.Genes
{
	public class SquareRootGene : FunctionGene
	{
		public const char Symbol = 'S';
		protected SquareRootGene(double multiple, IEnumerable<IGene> children) : base(Symbol, multiple, children)
		{
		}

		public SquareRootGene(double multiple = 1) : base(Symbol, multiple)
		{
		}

		public SquareRootGene(double multiple, IGene child) : this(multiple)
		{
			if (child != null)
				Add(child);
		}

		public SquareRootGene(IGene child) : this(1, child)
		{

		}

		public override void Add(IGene target)
		{
			if (GetChildren().Count == 1)
				throw new InvalidOperationException("A SquareRootGene can only have 1 child.");

			base.Add(target);
		}

		protected async override Task<double> CalculateWithoutMultiple(double[] values)
		{
			return Math.Sqrt(await GetChildren().Single().Calculate(values).ConfigureAwait(false));
		}

		public new SquareRootGene Clone()
		{
			return new SquareRootGene(Multiple, GetChildren().Select(g => g.Clone()));
		}

		protected override GeneticAlgorithmPlatform.IGene CloneInternal()
		{
			return this.Clone();
		}

		protected override void ReduceLoop()
		{
			var child = GetChildren().FirstOrDefault();
            if(child==null) return;            
			if (child.Multiple > 3)
			{
				// First migrate any possible multiple.
				var sqr = Math.Sqrt(child.Multiple);
				if (Math.Floor(sqr) == sqr)
				{
					if (child is ConstantGene)
						this.Clear();
					else
						child.Multiple = 1;

					this.Multiple *= sqr;
				}
			}
		}

		protected override IGene ReplaceWithReduced()
		{
            var child = GetChildren().FirstOrDefault();
            if(child==null) return null;       

            if(child.Multiple==0)
                return new ConstantGene(0);

			var product = child as ProductGene;
			if (product != null)
			{

				ProductGene wrapper = null;
				// Look for squares...
				foreach (var p in product.Children.ToArray()
					.GroupBy(g => g.ToStringContents())
					.Where(g => g.Count() > 1))
				{

					// Multiplying more than 1 square root of the same value together?
					var genes = p.ToList();

					while (genes.Count > 1)
					{
						if (wrapper == null) wrapper = new ProductGene();
						// Step 1 pull out the extra one.
						var last = genes.Last();
						Debug.Assert(last.Multiple == 1, "Should have already been pulled out.");
						genes.Remove(last);
						product.Remove(last);

						// Step 2 replace the square root container with the product.
						last = genes.Last();
						Debug.Assert(last.Multiple == 1, "Should have already been pulled out.");
						genes.Remove(last);
						product.Remove(last);
						wrapper.Add(last);
					}

				}

				if (wrapper != null)
				{
                    if(product.Children.Any())
                        wrapper.Add(this); // Assumes that the caller will replace this node with the wrapper.
                    return wrapper;
				}

			}
			return base.ReplaceWithReduced();
		}

	}
}