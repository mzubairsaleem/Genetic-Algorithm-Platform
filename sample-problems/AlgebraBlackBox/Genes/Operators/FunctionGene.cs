using System;
using System.Collections.Generic;

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
    }
}