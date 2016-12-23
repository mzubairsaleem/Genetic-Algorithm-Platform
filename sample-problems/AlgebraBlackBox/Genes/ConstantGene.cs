using System.Threading.Tasks;

namespace AlgebraBlackBox.Genes
{
    public class ConstantGene : UnreducibleGene
	{
		public ConstantGene(double multiple = 1) : base(multiple)
		{

		}


		public override string ToStringContents()
		{
			return string.Empty;
		}

		public new ConstantGene Clone()
		{
			return new ConstantGene(Multiple);
		}

        protected override GeneticAlgorithmPlatform.IGene CloneInternal()
        {
			return this.Clone();
        }

        protected override Task<double> CalculateWithoutMultiple(double[] values)
        {
			return Task.Run(()=>Multiple);
        }


		#region IEquatable<ConstantGene> Members

		public bool Equals(IGene other)
		{
			return other is ConstantGene && Multiple == other.Multiple;
		}

		public bool Equals(ConstantGene other)
		{
			return Multiple == other.Multiple;
		}

        public new ConstantGene AsReduced(bool ensureClone = false)
        {
            return ensureClone ? this.Clone(): this;
        }

        #endregion
    }
}
