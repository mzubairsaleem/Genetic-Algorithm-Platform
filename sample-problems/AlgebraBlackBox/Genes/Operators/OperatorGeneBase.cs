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
                Add(children);
        }

        public override bool IsReducible()
        {
            return true;
        }

        public new OperatorGeneBase AsReduced(bool ensureClone = false)
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
            foreach (var g in _children.OfType<OperatorGeneBase>().Where(g => g.Count == 0))
            {
                _children.Replace(g, new ConstantGene(g.Multiple));
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
            if (Operators.Available.Functions.Contains(Operator))
            {
                if (Count == 1)
                    return Operator + this.Single().ToString();
                else
                    return Operator + GroupedString(",");
            }
            return GroupedString(Operator);
        }

        string GroupedString(string separator)
        {
            return "(" + String.Join(separator, this.OrderBy(g => g).Select(s => s.ToString())) + ")";
        }
        string GroupedString(char separator)
        {
            return GroupedString(separator.ToString());
        }

        public new OperatorGeneBase Clone()
        {
            throw new NotImplementedException();
        }


    }
}
