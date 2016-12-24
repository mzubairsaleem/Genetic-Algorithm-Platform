using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBlackBox.Genes
{
    public class ParameterGene : ReducibleGene
	{

		static readonly Regex PATTERN = new Regex(@"(?<multiple>-?\d*){(?<id>\d+)}");

		public ParameterGene(int id, double multiple = 1) : base(multiple)
		{
			ID = id;
		}


		public ParameterGene(string pattern)
		{

			var m = PATTERN.Match(pattern);
			if(m==null || !m.Success)
				throw new ArgumentException("Unrecognized parameter pattern.");
			
			var g = m.Groups;
			Multiple = double.Parse(g["multiple"].Value);
			ID = int.Parse(g["id"].Value);
		}

		public int ID
		{
			get;
			private set;
		}


		public override string ToStringContents()
		{
			return "{" + ID + "}";
		}

		public new ParameterGene Clone()
		{
			return new ParameterGene(ID) { Multiple = Multiple };
		}

        public new ParameterGene AsReduced(bool ensureClone = false)
        {
            return ensureClone ? this.Clone(): this;
        }

        protected override GeneticAlgorithmPlatform.IGene CloneInternal()
        {
			return this.Clone();
        }

        protected override Task<double> CalculateWithoutMultiple(double[] values)
        {
            return Task.Run(()=>ID<values.Length ? values[ID] : double.NaN);
        }

		#region IEquatable<Gene> Members

		public bool Equals(ParameterGene other)
		{
			return this==other || ID==other.ID && Multiple==other.Multiple;
		}

        public override IGene Reduce()
        {
			if(this.Multiple==0
			|| double.IsInfinity(this.Multiple)
			|| double.IsNaN(this.Multiple))
				return new ConstantGene(this.Multiple);
				
			return null;
        }



        #endregion
    }
}
