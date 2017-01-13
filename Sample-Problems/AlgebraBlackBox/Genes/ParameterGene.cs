using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlgebraBlackBox.Genes
{
	public class ParameterGene : ReducibleGene
	{

		static readonly Regex PATTERN = new Regex(@"(?<multiple>-?\d*){(?<id>\d+)}");

		public ParameterGene(uint id, double multiple = 1) : base(multiple)
		{
			_id = id;
		}

		public ParameterGene(int id, double multiple = 1) : base(multiple)
		{
			if(id<0)
				throw new ArgumentOutOfRangeException("Must be at least zero.");
			_id = (uint)id;
		}

		public ParameterGene(string pattern) : base(1)
		{
			var m = PATTERN.Match(pattern);
			if (m == null || !m.Success)
				throw new ArgumentException("Unrecognized parameter pattern.");

			var g = m.Groups;
			Multiple = double.Parse(g["multiple"].Value);
			_id = uint.Parse(g["id"].Value);
		}

		private uint _id;
		public uint ID
		{
			get
			{
				AssertIsLiving();
				return _id;
			}
			private set
			{
				AssertIsLiving();
				Sync.Modifying(ref _id, value);
			}
		}


		public override string ToStringContents()
		{
			return "{" + ID + "}";
		}

		ParameterGene CloneThis()
		{
			return new ParameterGene(ID) { Multiple = Multiple };
		}

		public new ParameterGene Clone()
		{
			return CloneThis();
		}

		protected override GeneticAlgorithmPlatform.IGene CloneInternal()
		{
			return CloneThis();
		}

		protected override Task<double> CalculateWithoutMultipleAsync(double[] values)
		{
			return Task.FromResult(CalculateWithoutMultiple(values));
		}

		protected override double CalculateWithoutMultiple(double[] values)
		{
			AssertIsLiving();
			var id = ID;
			return id < values.Length ? values[id] : double.NaN;
		}

		public bool Equivalent(IGene other)
		{
			if (other == this) return true;
			var pg = other as ParameterGene;
			return pg != null && ID == pg.ID && Multiple == other.Multiple;
		}

	}
}
