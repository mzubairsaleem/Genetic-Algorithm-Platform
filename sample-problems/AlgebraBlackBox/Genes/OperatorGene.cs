using System;
using System.Collections.Generic;
using System.Linq;
using Open.Math;

namespace AlgebraBlackBox.Genes
{
    public partial class OperatorGene : Gene<Gene>
    {


        public OperatorGene(
            char op,
            double multiple = 1,
            IEnumerable<Gene> children = null) : base(multiple)
        {
            _operator = op;
            if (children != null)
                Add(children);
        }

        public override bool IsReducible()
        {
            return true;
        }

        public new OperatorGene AsReduced(bool ensureClone = false)
        {
            var gene = this.Clone();
            gene.Reduce();
            return gene;
        }

        char _operator;
        public char Operator
        {
            get { return _operator; }
            set
            {
                if (_operator != value)
                {
                    _operator = value;
                    OnModified();
                }
            }
        }

        public override string ToStringContents()
        {
            if (Available.Functions.Contains(Operator))
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

        public new OperatorGene Clone()
        {
            return new OperatorGene(Operator, Multiple, Children.Select(g => g.Clone()));
        }

        public static class Available
        {
            public static readonly char[] Operators = new char[] { '+', '*', '/' };
            public static readonly char[] Functions = new char[] { '√' };
        }


        public static char GetRandomOperator(IEnumerable<char> excluded = null)
        {
            var ao = excluded == null
                ? Available.Operators
                : Available.Operators.Where(o => !excluded.Contains(o)).ToArray();
            return ao[Environment.Randomizer.Next(ao.Length)];
        }

        public static char GetRandomOperator(char excluded)
        {
            var ao = Available.Operators.Where(o => o != excluded).ToArray();
            return ao[Environment.Randomizer.Next(ao.Length)];
        }

        public static char GetRandomFunctionOperator()
        {
            return Available.Functions[Environment.Randomizer.Next(Available.Functions.Length)];
        }

        public static OperatorGene GetRandomOperation(IEnumerable<char> excluded = null)
        {
            return new OperatorGene(GetRandomOperator(excluded));
        }

        public static OperatorGene GetRandomOperation(char excluded)
        {
            return new OperatorGene(GetRandomOperator(excluded));
        }

        public static OperatorGene GetRandomFunction()
        {
            return new OperatorGene(GetRandomFunctionOperator());
        }

        protected override double CalculateWithoutMultiple(double[] values)
        {
            var results = ((IList<Gene>)this).Select(s => s.Calculate(values)).ToArray();
            switch (Operator)
            {

                case '+':
                    return results.Any() ? results.Sum() : 0;

                // For now "Power Of" functions will be elaborated with a*a.
                case '*':
                    return results.Product();

                case '/':
                    return results.Quotient();

                // Single value functions...
                case '√':
                    if (results.Length != 1)
                        return double.NaN;

                    return Math.Sqrt(results[0]);
            }

            return double.NaN;
        }

    }
}
