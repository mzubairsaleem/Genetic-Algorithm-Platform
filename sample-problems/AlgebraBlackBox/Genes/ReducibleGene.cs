/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;

namespace AlgebraBlackBox.Genes
{

    public abstract class ReducibleGeneNode : GeneNode, IReducibleGene
    {

        public ReducibleGeneNode(double multiple = 1) : base(multiple)
        {

        }

        override protected void OnModified()
        {
            base.OnModified();
            if(_reduced==null || _reduced.IsValueCreated)
                _reduced = Lazy.New(()=>this.Clone().Reduce() ?? this); // Use a clone to prevent any threading issues.
        }

        public new ReducibleGeneNode Clone()
        {
            return (ReducibleGeneNode)CloneInternal();
        }

        Lazy<IGene> _reduced;

        public bool IsReducible
        {
            get
            {
                return _reduced.Value!=this;
            }
        }

        public IGene AsReduced(bool ensureClone = false)
        {
            var r = _reduced.Value;
            return ensureClone ? r.Clone() : r;
        }

        public abstract IGene Reduce();
    }

    public abstract class ReducibleGene : Gene, IReducibleGene {

        public ReducibleGene(double multiple = 1) : base(multiple)
        {

        }


        override protected void OnModified()
        {
            base.OnModified();
            if(_reduced==null || _reduced.IsValueCreated)
                _reduced = Lazy.New(()=>this.Clone().Reduce() ?? this); // Use a clone to prevent any threading issues.
        }

        Lazy<IGene> _reduced;

        public bool IsReducible
        {
            get
            {
                return _reduced.Value!=this;
            }
        }

        public new ReducibleGene Clone()
        {
            return (ReducibleGene)CloneInternal();
        }


        public IGene AsReduced(bool ensureClone = false)
        {
            var r = _reduced.Value;
            return ensureClone ? r.Clone() : r;
        }

        public abstract IGene Reduce();
    }

}
