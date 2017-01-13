using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraBlackBox.Genes
{
    public abstract class FunctionGene : OperatorGeneBase
    {
        public FunctionGene(char op, double multiple = 1) : base(op, multiple)
        {
        }

        public FunctionGene(char op, double multiple, IEnumerable<IGene> children) : base(op, multiple, children)
        {
        }

        public FunctionGene(char op, double multiple, IGene child) : base(op, multiple, child)
        {
        }

        protected override double DefaultIfNoChildren()
        {
            throw new InvalidOperationException("Cannot calculate a FunctionGene with no child.");
        }

        public new FunctionGene Clone()
        {
            throw new NotImplementedException();
        }


        public override string ToStringContents()
        {
            if (Count == 1)
                return Operator + this.Single().ToString();
            else
                return Operator + GroupedString(",");
        }
    }
}