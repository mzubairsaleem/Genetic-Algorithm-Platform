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

        protected override Task<double> CalculateWithoutMultiple(double[] values)
        {
			return Task.Run(()=>Multiple);
        }

		public bool Equivalent(IGene other)
		{
			return this==other || other is ConstantGene && Multiple == other.Multiple;
		}
    }
}
