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

        public override double Calculate(double[] values)
        {
            return Multiple;
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

        #endregion
    }
}
