using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneticAlgorithmPlatform;

namespace AlgebraBlackBox.Genes
{
    public class SquareRootGene : OperatorGeneBase
    {
        public const char Symbol = '√';
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
            if (_children.Count == 1)
                throw new InvalidOperationException("A SquareRootGene can only have 1 child.");

            base.Add(target);
        }

        protected async override Task<double> CalculateWithoutMultiple(double[] values)
        {
            return Math.Sqrt(await _children.Single().Calculate(values));
        }

        public new DivisionGene Clone()
        {
            return new DivisionGene(Multiple, _children.Select(g => g.Clone()));
        }

        protected override GeneticAlgorithmPlatform.IGene CloneInternal()
        {
            return this.Clone();
        }

        override protected void ReduceLoop()
        {
            var child = _children.SingleOrDefault();
            if (child != null && child.Multiple > 3)
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

            base.ReduceLoop();
        }


    }
}