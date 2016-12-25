
using System;

namespace Open.Arithmetic
{
	public struct ProcedureResult : IComparable<ProcedureResult>
	{
		public readonly int Count;
		public readonly double Sum;
		public readonly double Average;
		public ProcedureResult(double sum, int count)
		{
			Count = count;
			Sum = sum;
			Average = sum / count;
		}

		public ProcedureResult Add(double value, int count = 1)
		{
			return new ProcedureResult(Sum+value,Count+count);
		}

		public int CompareTo(ProcedureResult other)
		{
			var a = Average;
			var b = other.Average;
			if (a < b || double.IsNaN(a) && !double.IsNaN(b)) return -1;
			if (a > b || !double.IsNaN(a) && double.IsNaN(b)) return +1;
			if (Count < other.Count) return -1;
			if (Count > other.Count) return +1;
			return 0;
		}

	}
}