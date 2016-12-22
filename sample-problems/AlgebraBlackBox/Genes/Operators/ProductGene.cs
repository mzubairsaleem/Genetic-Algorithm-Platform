using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Open.Math;

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
            if (_children.Count == 0) return 1;
            var results = await Task.WhenAll( _children.Select(s => s.Calculate(values)));
            return results.Product();
        }

        public new ProductGene Clone()
        {
            return new ProductGene(Multiple, _children.Select(g => g.Clone()));
        }

        void MigrateMultiples()
        {
            if (_children.Any(c => c.Multiple == 0))
            {
                // Any multiple of zero? Reset the entire collection;
                Clear();
                Multiple = 0;
            }
            else
            {
                // Extract any multiples so we don't have to worry about them later.
                foreach (var c in _children.OfType<ConstantGene>())
                {
                    var m = c.Multiple;
                    Remove(c);
                    this.Multiple *= m;
                }
                foreach (var c in _children)
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

            foreach (var p in _children.OfType<DivisionGene>())
            {
                Debug.Assert(p.Multiple == 1, "Should have already been pulled out.");
                // Use the internal reduction routine of the division gene to reduce the division node.
                var m = Multiple;
                p.Multiple = m;
                if (p.Reduce())
                    Sync.IncrementVersion();
                Debug.Assert(Multiple == m, "Shouldn't have changed!");
                Multiple = p.Multiple;
                p.Multiple = 1;
            }

            // Collapse products within products.
            foreach (var p in _children.OfType<ProductGene>())
            {
                Debug.Assert(p.Multiple == 1, "Should have already been pulled out.");
                _children.Add(p.Children);
                p.Clear();
                _children.Remove(p);
            }

            // Look for groupings...
            foreach (var p in _children
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
                    Replace(last, last.Single()); // It should only be single.. If not, we have a serious problem somewhere else.
                }

                break;

            }

            // Look for squares that have been rooted.
            foreach (var sr in _children
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


            base.ReduceLoop();

        }
    }
}