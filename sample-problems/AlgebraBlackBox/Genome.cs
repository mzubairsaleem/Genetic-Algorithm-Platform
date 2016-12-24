using System;
using System.Threading.Tasks;

namespace AlgebraBlackBox
{
    public class Genome : GeneticAlgorithmPlatform.Genome<IGene>, IEquatable<Genome>, IReducible<Genome>
    {
        public Genome(IGene root) : base(root)
        {
			OnModified();
        }

        protected void OnModified()
        {
            if (_reduced == null || _reduced.IsValueCreated)
                _reduced = Lazy.New(() =>
                {
                    var r = Root as IReducibleGene;
                    return r != null && r.IsReducible ? new Genome(r.AsReduced()) : this;
                }); // Use a clone to prevent any threading issues.
        }


        public new IGeneNode FindParent(IGene child)
        {
            return (IGeneNode)base.FindParent(child);
        }

        public void Replace(IGene target, IGene replacement)
        {
            if (target != replacement)
            {
                if (Root == target)
                {
                    Root = replacement;
                    OnModified();
                }
                else
                {
                    var parent = FindParent(target);
                    if (parent == null)
                        throw new ArgumentException("'target' not found.");
                    if (parent.Replace(target, replacement))
                    {
                        OnModified();
                    }
                }
            }

        }

        public override string ToString()
        {
            return Root.ToString();
        }



        public new Genome Clone()
        {
            return new Genome(Root.Clone());
        }

        public Task<double> Calculate(double[] values)
        {
            return Root.Calculate(values);
        }


        public string ToAlphaParameters(bool reduced = false)
        {
            return AlphaParameters.ConvertTo(reduced ? AsReduced().ToString() : ToString());
        }



        #region IEquatable<Genome> Members

        public bool Equals(Genome other)
        {
            return this == other || this.ToString() == other.ToString();
        }

        #endregion

        Lazy<Genome> _reduced;

        public bool IsReducible
        {
            get
            {
                return _reduced.Value != this;
            }
        }

        public Genome AsReduced(bool ensureClone = false)
        {
            var r = _reduced.Value;
            return ensureClone ? r.Clone() : r;
        }
    }
}
