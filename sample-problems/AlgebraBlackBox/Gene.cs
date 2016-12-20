using System;
using GeneticAlgorithmPlatform;

namespace AlgebraBlackBox
{


    public abstract class Gene : GeneBase, IGene
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
            var changing = _multiple != value;
            if (changing)
                _multiple = value;
            return changing;
        }
        public abstract double Calculate(double[] values);


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
    }

    public abstract class Gene<T> : GeneBase<T>, IGene
    where T : IGene
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
            var changing = _multiple != value;
            if (changing)
                _multiple = value;
            return changing;
        }


        protected override string ToStringInternal()
        {
            return MultiplePrefix + ToStringContents();
        }

        public abstract string ToStringContents();


        public string ToAlphaParameters()
        {
            return AlphaParameters.ConvertTo(this.ToString());
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


        public double Calculate(double[] values)
        {
            return Multiple * CalculateWithoutMultiple(values);
        }

        protected abstract double CalculateWithoutMultiple(double[] values);


        public abstract bool IsReducible();

        public virtual int CompareTo(IGene other)
        {
           return this.Compare(other);
        }

        public new Gene<T> Clone()
        {
            throw new NotImplementedException();
        }

        public virtual IGene AsReduced(bool ensureClone = false)
        {
            throw new NotImplementedException();
        }
    }
}
