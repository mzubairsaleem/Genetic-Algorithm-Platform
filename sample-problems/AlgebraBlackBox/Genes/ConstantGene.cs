using System.Threading.Tasks;

namespace AlgebraBlackBox.Genes
{
    public class ConstantGene : Gene
	{
		public ConstantGene(double multiple = 1) : base(multiple)
		{

		}


		public override string ToStringContents()
		{
			return string.Empty;
		}

		protected override string ToStringInternal()
		{
			return Multiple.ToString();
		}

		public new ConstantGene Clone()
		{
			return new ConstantGene(Multiple);
		}

        protected override GeneticAlgorithmPlatform.IGene CloneInternal()
        {
			return this.Clone();
        }

		static readonly Task<double> T1d = Task.FromResult(1d);
        protected override Task<double> CalculateWithoutMultipleAsync(double[] values)
        {
			return T1d;
        }

		protected override double CalculateWithoutMultiple(double[] values)
        {
			return 1d;
        }

		public bool Equivalent(IGene other)
		{
			return this==other || other is ConstantGene && Multiple == other.Multiple;
		}
    }
}
