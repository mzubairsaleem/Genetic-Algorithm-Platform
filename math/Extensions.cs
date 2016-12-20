using System;
using System.Collections.Generic;

namespace Open.Math
{
    public static class Extensions
	{
		public static int PowerOf(this int value, int power)
		{
			if (power < 0)
				throw new ArgumentOutOfRangeException();
			var v = 1;
			for (var i = 1; i < power; i++)
				v *= value;
			return v;
		}

		public static int AsInteger(this bool[] source)
		{
			if(source.Length>32)
				throw new ArgumentOutOfRangeException();

			int result = 0;

			for (int i = 0; i < source.Length; i++)
				if (source[i])
					result += 2.PowerOf(i);

			return result;
		}

		public static double Product(this IEnumerable<double> source)
		{
			var any = false;
			var result = 1d;
			foreach (var s in source)
			{
				any = true;
				result *= s;
			}

			return any ? result : double.NaN;
		}

		public static double Quotient(this IEnumerable<double> source)
		{
			var any = false;
			var result = 1d;
			foreach (var s in source)
			{
				if (!any)
					result = s;
				else
					result /= s;

				any = true;

			}

			return any ? result : double.NaN;
		}

		public static double Difference(this IEnumerable<double> source)
		{
			var any = false;
			var result = 1d;
			foreach (var s in source)
			{
				if (!any)
					result = s;
				else
					result -= s;

				any = true;

			}

			return any ? result : double.NaN;
		}

	}
}
