using System;
using System.Threading.Tasks;
using GeneticAlgorithmPlatform;

namespace AlgebraBlackBox
{


    public abstract class Gene : GeneBase, AlgebraBlackBox.IGene
    {
        public Gene(double multiple = 1) : base()
        {
            Multiple = multiple;
        }

        double _multiple;
        public double Multiple
        {
            get { return _multiple; }
            set
            {
                SetMultiple(value);
            }
        }

        public bool SetMultiple(double value)
        {
            return Sync.Modifying(ref _multiple, value);
        }
        public virtual async Task<double> Calculate(double[] values)
        {
            return Multiple * await CalculateWithoutMultiple(values);
        }

        protected abstract Task<double> CalculateWithoutMultiple(double[] values);


        public virtual IGene AsReduced(bool ensureClone = false)
        {
            // This way we can easily override and specify a reduction and detect if no reduction implemented.
            throw new NotImplementedException();
        }

        protected string MultiplePrefix
        {
            get
            {
                if (Multiple != 1d)
                    return Multiple == -1d ? "-" : Multiple.ToString();

                return String.Empty;
            }
        }

        protected override string ToStringInternal()
        {
            return MultiplePrefix + ToStringContents();
        }

        public abstract string ToStringContents();

        public abstract bool IsReducible();

        public virtual int CompareTo(IGene other)
        {
            return this.Compare(other);
        }

        public new Gene Clone()
        {
            throw new NotImplementedException();
        }

        IGene IGene.Clone()
        {
            return this.Clone(); ;
        }

        IGene ICloneable<IGene>.Clone()
        {
            return this.Clone();
        }

    }

    public abstract class GeneNode : GeneBase<AlgebraBlackBox.IGene>, IGeneNode
    {
        public GeneNode(double multiple = 1) : base()
        {
            Multiple = multiple;
        }


        double _multiple;
        public double Multiple
        {
            get { return _multiple; }
            set
            {
                SetMultiple(value);
            }
        }

        public bool SetMultiple(double value)
        {
            return Sync.Modifying(ref _multiple, value);
        }


        protected override string ToStringInternal()
        {
            return MultiplePrefix + ToStringContents();
        }

        public abstract string ToStringContents();

        protected string MultiplePrefix
        {
            get
            {
                if (Multiple != 1d)
                    return Multiple == -1d ? "-" : Multiple.ToString();

                return String.Empty;
            }
        }


        public virtual async Task<double> Calculate(double[] values)
        {
            return Multiple * await CalculateWithoutMultiple(values);
        }

        protected abstract Task<double> CalculateWithoutMultiple(double[] values);


        public abstract bool IsReducible();

        public virtual int CompareTo(IGene other)
        {
            return this.Compare(other);
        }

        public new GeneNode Clone()
        {
            throw new NotImplementedException();
        }

        public virtual IGene AsReduced(bool ensureClone = false)
        {
            throw new NotImplementedException();
        }

        IGene IGene.Clone()
        {
            return this.Clone(); ;
        }

        IGene ICloneable<IGene>.Clone()
        {
            return this.Clone();
        }
    }
}
