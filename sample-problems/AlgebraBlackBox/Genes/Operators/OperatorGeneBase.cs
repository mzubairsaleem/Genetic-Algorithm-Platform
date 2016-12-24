using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraBlackBox.Genes
{
    public abstract class OperatorGeneBase : ReducibleGeneNode
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

        /*
            REDUCTION:
            Goal, avoid reference to parent so that nodes can be reused across genes.
            When calling .Reduce() a gene will be returned as a gene that could replace this one,
            or the same gene is returned signaling that it's contents were updated.
        */

        protected IGene ChildReduce(OperatorGeneBase child)
        {
            // Here's the magic... If the Reduce call returns non-null, then attempt to replace o with (new) g.
            var g = child.Reduce(); 
            if (g != null)
            {                            
                _children.Replace(child, g);
                Sync.IncrementVersion();
            }
            return g;
        }

        public override IGene Reduce()
        {
            IGene reduced = this;
            var modfied = Sync.Modifying(() =>
            {
                // This could be excessive and there definitely could be optimizations, but for now this will do.
                int ver;
                do
                {
                    ver = Version;

                    foreach (var o in _children.OfType<OperatorGeneBase>().ToArray())
                    {
                        ChildReduce(o);
                    }

                    ReduceLoop();
                }
                while (ver != Version);

            });

            return ReplaceWithReduced()
                ?? (modfied ? reduced : null);
        }

        // Call this at the end of the sub-classes reduce loop.
        protected abstract void ReduceLoop();

        protected virtual IGene ReplaceWithReduced()
        {
            return _children.Count==0 ? new ConstantGene(this.Multiple) : null;
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
