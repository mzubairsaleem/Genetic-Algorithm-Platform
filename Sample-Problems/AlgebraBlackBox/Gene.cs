using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneticAlgorithmPlatform;

namespace AlgebraBlackBox
{

	public abstract class Gene : GeneBase, AlgebraBlackBox.IGene
	{
		public Gene(double multiple = 1) : base()
		{
			Multiple = multiple;
		}

		double _multiple;
		public double Multiple
		{
			get
			{
				AssertIsLiving();
				return Sync.Reading(() => _multiple);
			}
			set
			{
				SetMultiple(value);
			}
		}

		public bool SetMultiple(double value)
		{
			AssertIsLiving();
			return Sync.Modifying(ref _multiple, value);
		}

		public double Calculate(IReadOnlyList<double> values)
		{
			AssertIsLiving();
			var m = Multiple;
			if (m == 0) return 0; // zero==zero even if the underlying calc is NaN.
			return m * CalculateWithoutMultiple(values);
		}

		public async Task<double> CalculateAsync(IReadOnlyList<double> values)
		{
			AssertIsLiving();
			var m = Multiple;
			if (m == 0) return 0; // zero==zero even if the underlying calc is NaN.
			var calc = await CalculateWithoutMultipleAsync(values);
			return m * calc;
		}

		protected abstract double CalculateWithoutMultiple(IReadOnlyList<double> values);
		protected abstract Task<double> CalculateWithoutMultipleAsync(IReadOnlyList<double> values);

		protected string MultiplePrefixFrom(double m)
		{
			if (m != 1d)
				return m == -1d ? "-" : m.ToString();

			return String.Empty;
		}

		protected string MultiplePrefix
		{
			get
			{
				return MultiplePrefixFrom(Multiple);
			}
		}

		public virtual string ToStringUsingMultiple(double m)
		{
			return MultiplePrefixFrom(m) + ToStringContents();
		}

		protected override string ToStringInternal()
		{
			return MultiplePrefix + ToStringContents();
		}

		public abstract string ToStringContents();

		public virtual int CompareTo(IGene other)
		{
			return this.Compare(other);
		}

		public new Gene Clone()
		{
			return (Gene)CloneInternal();
		}


		IGene IGene.Clone()
		{
			return this.Clone();
		}

		IGene ICloneable<IGene>.Clone()
		{
			return this.Clone();
		}

	}

	public abstract class GeneNode : GeneBase<AlgebraBlackBox.IGene>, IGeneNode
	{
		public GeneNode(double multiple = 1) : base()
		{
			Multiple = multiple;
		}


		double _multiple;
		public double Multiple
		{
			get
			{
				AssertIsLiving();
				return Sync.Reading(() => _multiple);
			}
			set
			{
				SetMultiple(value);
			}
		}


		public bool SetMultiple(double value)
		{
			AssertIsLiving();
			return Sync.Modifying(ref _multiple, value);
		}

		protected override string ToStringInternal()
		{
			return MultiplePrefix + ToStringContents();
		}

		public virtual string ToStringUsingMultiple(double m)
		{
			return MultiplePrefixFrom(m) + ToStringContents();
		}


		public abstract string ToStringContents();

		protected string MultiplePrefixFrom(double m)
		{
			if (m != 1d)
				return m == -1d ? "-" : m.ToString();

			return String.Empty;
		}

		protected string MultiplePrefix
		{
			get
			{
				return MultiplePrefixFrom(Multiple);
			}
		}



		public double Calculate(IReadOnlyList<double> values)
		{
			AssertIsLiving();
			var m = Multiple;
			if (m == 0) return 0; // zero==zero even if the underlying calc is NaN.
			return m * CalculateWithoutMultiple(values);
		}

		public async Task<double> CalculateAsync(IReadOnlyList<double> values)
		{
			AssertIsLiving();
			var m = Multiple;
			if (m == 0) return 0; // zero==zero even if the underlying calc is NaN.
			var calc = await CalculateWithoutMultipleAsync(values);
			return m * calc;
		}

		protected double CalculateWithoutMultiple(IReadOnlyList<double> values)
		{
			if (GetChildren().Count == 0) return DefaultIfNoChildren();
			return ProcessChildValues(CalculateChildren(values));
		}

		protected Task<double> CalculateWithoutMultipleAsync(IReadOnlyList<double> values)
		{
			if (GetChildren().Count == 0) return Task.FromResult(DefaultIfNoChildren());
			return Task.WhenAll(CalculateChildrenAsync(values))
				.ContinueWith(task => ProcessChildValues(task.Result));
		}

		protected abstract double DefaultIfNoChildren();
		protected abstract double ProcessChildValues(IEnumerable<double> values);

		protected IEnumerable<double> CalculateChildren(IReadOnlyList<double> values)
		{
			return GetChildren().Select(s => s.Calculate(values));
		}
		protected IEnumerable<Task<double>> CalculateChildrenAsync(IReadOnlyList<double> values)
		{
			return GetChildren().Select(s => s.CalculateAsync(values));
		}

		public virtual int CompareTo(IGene other)
		{
			return this.Compare(other);
		}

		public new GeneNode Clone()
		{
			return (GeneNode)CloneInternal();
		}

		IGene IGene.Clone()
		{
			return this.Clone(); ;
		}

		IGene ICloneable<IGene>.Clone()
		{
			return this.Clone();
		}

	}
}
