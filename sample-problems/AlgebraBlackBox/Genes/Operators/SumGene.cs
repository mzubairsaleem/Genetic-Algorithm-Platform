using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.Math;

namespace AlgebraBlackBox.Genes
{
    public class SumGene : OperatorGeneBase
    {
        public const char Symbol = '+';

        public SumGene(double multiple = 1, IEnumerable<IGene> children = null) : base(Symbol, multiple, children)
        {
        }

        protected async override Task<double> CalculateWithoutMultiple(double[] values)
        {
            using (var results = _children.Select(s => s.Calculate(values)).Memoize())
            {
                return results.Any() ? (await Task.WhenAll(results)).Sum() : 0;
            }
        }

        public new SumGene Clone()
        {
            return new SumGene(Multiple, _children.Select(g => g.Clone()));
        }


        void RemoveZeroMultiples()
        {
            foreach (var g in _children.Where(g => g.Multiple == 0).ToArray())
            {
                Remove(g);
            }
        }

        protected override void ReduceLoop()
        {
            // Collapse sums within sums.
            foreach (var p in _children.OfType<ProductGene>().ToArray())
            {
                var m = p.Multiple;
                foreach (var s in p)
                {
                    s.Multiple *= m;
                    _children.Add(s);
                }
                p.Clear();
                _children.Remove(p);
            }

            // Pull out multiples.
            if (_children.All(g => Math.Abs(g.Multiple) > 1))
            {
                var smallest = _children.OrderBy(g => g.Multiple).First();
                var max = smallest.Multiple;
                for (var i = 2; i <= max; i = i.NextPrime())
                {
                    while (max % i == 0 && _children.All(g => g.Multiple % i == 0))
                    {
                        max /= i;
                        Multiple *= i;
                        foreach (var g in _children)
                        {
                            g.Multiple /= i;
                        }
                    }
                }

            }

            // Combine any constants.  This is more optimal because we don't neet to query ToStringContents.
            using (var constants = _children.OfType<ConstantGene>().Memoize())
            {
                if (constants.Count > 1)
                {
                    var main = constants.First();
                    foreach (var c in constants.Skip(1))
                    {
                        main.Multiple += c.Multiple;
                        _children.Remove(c);
                    }
                }
            }

            RemoveZeroMultiples();

            // Look for groupings...
            foreach (var p in _children
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

            base.ReduceLoop();
        }

        protected override GeneticAlgorithmPlatform.IGene CloneInternal()
        {
            return this.Clone();
        }
    }
}
