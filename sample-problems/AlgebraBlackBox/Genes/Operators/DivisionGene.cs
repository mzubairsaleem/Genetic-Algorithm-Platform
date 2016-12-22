using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.Math;

namespace AlgebraBlackBox.Genes
{
    public class DivisionGene : OperatorGeneBase
    {
        public const char Symbol = '/';

        public DivisionGene(double multiple = 1, IEnumerable<IGene> children = null) : base(Symbol, multiple, children)
        {
        }

        protected async override Task<double> CalculateWithoutMultiple(double[] values)
        {
            // Allow for special case which will get cleaned up.
            if (_children.Count == 0) return 1;
            var results = await Task.WhenAll( _children.Select(s => s.Calculate(values)));
            return results.QuotientOf(1);
        }

        public new DivisionGene Clone()
        {
            return new DivisionGene(Multiple, _children.Select(g => g.Clone()));
        }

        override protected void ReduceLoop()
        {
            // Pull out clean divisors.
            foreach(var g in _children.ToArray())
            {
                var m = g.Multiple;
                if(Multiple%m==0)
                {
                    g.Multiple = 1;
                    if(g is ConstantGene)
                        _children.Remove(g);
                    Multiple /= m;
                }
            }

            base.ReduceLoop();
        }

    }
}