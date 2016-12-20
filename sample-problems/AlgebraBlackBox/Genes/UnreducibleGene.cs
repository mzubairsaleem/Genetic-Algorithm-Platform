/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;

namespace AlgebraBlackBox.Genes
{

    public abstract class UnreducibleGene<T> : Gene<T>
    where T : Gene<T>
    {

        public UnreducibleGene(double multiple = 1) : base(multiple)
        {

        }

        public override bool IsReducible()
        {
            return false;
        }

        public new UnreducibleGene<T> Clone()
        {
            throw new NotImplementedException();
        }

        public new virtual UnreducibleGene<T> AsReduced(bool ensureClone = false)
        {
            return ensureClone ? this.Clone(): this;
        }

    }

    public abstract class UnreducibleGene : Gene {

        public UnreducibleGene(double multiple = 1) : base(multiple)
        {

        }

        public override bool IsReducible()
        {
            return false;
        }

        public new UnreducibleGene Clone()
        {
            throw new NotImplementedException();
        }

        public new virtual UnreducibleGene AsReduced(bool ensureClone = false)
        {
            return ensureClone ? this.Clone(): this;
        }     
    }

}
