using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraBlackBox.Genes
{
    public abstract class OperatorGeneBase : GeneNode
    {


        protected OperatorGeneBase(
            char op,
            double multiple = 1,
            IEnumerable<AlgebraBlackBox.IGene> children = null) : base(multiple)
        {
            _operator = op;
            if (children != null)
                AddThese(children);
        }

        public override bool IsReducible()
        {
            return true;
        }

        public override IGene AsReduced(bool ensureClone = false)
        {
            var gene = this.Clone();
            gene.Reduce();
            return gene;
        }

        public bool Reduce()
        {
            return Sync.Modifying(() =>
            {
                // This could be excessive and there definitely could be optimizations, but for now this will do.
                int ver;
                do
                {
                    foreach (var o in _children.OfType<OperatorGeneBase>())
                    {
                        if (o.Reduce())
                            Sync.IncrementVersion();
                    }
                    ver = Version;
                    ReduceLoop();
                }
                while (ver != Version);
            });
        }

        // Call this at the end of the sub-classes reduce loop.
        protected virtual void ReduceLoop()
        {
            // Convert empty operators to their constant counterparts.
            foreach (var g in _children.OfType<OperatorGeneBase>().ToArray())
            {
                switch (g.Count)
                {
                    case 0:
                        _children.Replace(g, new ConstantGene(g.Multiple));
                        break;
                    case 1:
                        if(g is ProductGene || g is SumGene)
                        {
                            var c = g.Single();
                            c.Multiple *= g.Multiple;
                            _children.Replace(g, c);
                        }
                        break;
                }

            }
        }


        char _operator;
        public char Operator
        {
            get { return _operator; }
            protected set
            {
                SetOperator(value);
            }
        }

        protected bool SetOperator(char value)
        {
            return Sync.Modifying(ref _operator, value);
        }

        public override string ToStringContents()
        {
            return GroupedString(Operator);
        }

        protected string GroupedString(string separator, string internalPrefix = "")
        {
            return "(" + internalPrefix + String.Join(separator, this.OrderBy(g => g).Select(s => s.ToString())) + ")";
        }
        protected string GroupedString(char separator, string internalPrefix = "")
        {
            return GroupedString(separator.ToString(), internalPrefix);
        }

        public new OperatorGeneBase Clone()
        {
            return (OperatorGeneBase)CloneInternal();
        }


    }
}
