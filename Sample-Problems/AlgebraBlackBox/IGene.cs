using System;
using AlgebraBlackBox.Genes;
using Open.Threading;

namespace AlgebraBlackBox
{
    public interface IGene : GeneticAlgorithmPlatform.ICalculableGene<double>, IComparable<IGene>, ICloneable<IGene>
    {

        string ToStringContents();

        string ToStringUsingMultiple(double m);

        IModificationSynchronizer Sync { get; }

        new IGene Clone();
    }

    public interface IGeneNode<T> : GeneticAlgorithmPlatform.IGeneNode<T>, IGene
    where T : IGene
    {
    }

    public interface IGeneNode : IGeneNode<IGene>
    {

    }

    static class GeneUtil
    {
        public static int Compare(this IGene a, IGene b)
        {
            if(a.Multiple<0 && b.Multiple>0)
                return 1;
                
            if(a.Multiple>0 && b.Multiple<0)
                return -1;

            // Constants should trail at the end.
            if (a is ConstantGene && !(b is ConstantGene))
                return 1;

            if (b is ConstantGene && !(a is ConstantGene))
                return -1;

            if (a is ParameterGene && !(b is ParameterGene))
                return 1;

            if (b is ParameterGene && !(a is ParameterGene))
                return -1;

            // Has no children?
            if (a is Gene && !(b is Gene))
                return 1;

            if (b is Gene && !(a is Gene))
                return -1;

            if (a.Multiple < b.Multiple)
                return 1;

            if (a.Multiple > b.Multiple)
                return -1;

            var ats = a.ToString();
            var bts = b.ToString();

            return String.Compare(ats, bts);
        }
    }

}