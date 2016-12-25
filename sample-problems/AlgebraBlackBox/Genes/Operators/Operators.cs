using System;
using System.Collections.Generic;
using System.Linq;
using AlgebraBlackBox.Genes;
using Open;

namespace AlgebraBlackBox
{
    public static class Operators
    {
        public const char ADD = SumGene.Symbol;
        public const char MULTIPLY = ProductGene.Symbol;
        public const char DIVIDE = DivisionGene.Symbol;
        public const char SQUARE_ROOT = SquareRootGene.Symbol;

        public static class Available
        {
            public static readonly IReadOnlyList<char> Operators = (new List<char> { ADD, MULTIPLY, DIVIDE }).AsReadOnly();
            public static readonly IReadOnlyList<char> Functions = (new List<char> { SQUARE_ROOT }).AsReadOnly();
        }

        public static OperatorGeneBase New(char op, double multiple = 1)
        {

            switch (op)
            {

                case ADD:
                    return new SumGene(multiple);
                    
                case MULTIPLY:
                    return new ProductGene(multiple);

                case DIVIDE:
                    return new DivisionGene(multiple);

                case SQUARE_ROOT:
                    return new SquareRootGene(multiple);
            }

            throw new ArgumentException("Invalid operator symbol.", "op");

        }


        

        public static char GetRandom(IEnumerable<char> excluded = null)
        {
            var ao = excluded == null
                ? Available.Operators
                : Available.Operators.Where(o => !excluded.Contains(o)).ToArray();
            return ao.RandomSelectOne();
        }

        public static char GetRandom(char excluded)
        {
            var ao = Available.Operators.Where(o => o != excluded).ToArray();
            return ao.RandomSelectOne();
        }

        public static char GetRandomFunction()
        {
            return Available.Functions.RandomSelectOne();
        }

        public static char GetRandomFunction(char excluded)
        {
            var ao = Available.Functions.Where(o => o != excluded).ToArray();
            return ao.RandomSelectOne();
        }

        public static OperatorGeneBase GetRandomOperationGene(IEnumerable<char> excluded = null)
        {
            return New(GetRandom(excluded));
        }

        public static OperatorGeneBase GetRandomOperationGene(char excluded)
        {
            return New(GetRandom(excluded));
        }

        public static OperatorGeneBase GetRandomFunctionGene(char excluded)
        {
            return New(GetRandomFunction(excluded));
        }


    }
}