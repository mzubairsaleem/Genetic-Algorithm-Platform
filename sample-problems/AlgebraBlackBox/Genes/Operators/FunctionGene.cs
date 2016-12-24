using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraBlackBox.Genes
{
    public abstract class FunctionGene : OperatorGeneBase
    {
        public FunctionGene(char op, double multiple = 1, IEnumerable<IGene> children = null) : base(op, multiple, children)
        {
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