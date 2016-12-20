using System;
using System.Text.RegularExpressions;

namespace AlgebraBlackBox.Genes
{
    public class ParameterGene : UnreducibleGene
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

		public override double Calculate(double[] values)
		{
			return ID<values.Length ? values[ID] : double.NaN;
		}

		#region IEquatable<Gene> Members

		public bool Equals(ParameterGene other)
		{
			return this==other || ID==other.ID && Multiple==other.Multiple;
		}

        #endregion
    }
}
