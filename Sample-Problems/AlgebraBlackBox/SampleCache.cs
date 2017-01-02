using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Open;

namespace AlgebraBlackBox
{
	public sealed class SampleCache
	{
		Formula _actualFormula;
		ConcurrentDictionary<int, KeyValuePair<double[], double>[]> _sampleCache;

		public SampleCache(Formula actualFormula)
		{
			_actualFormula = actualFormula;
			_sampleCache = new ConcurrentDictionary<int, KeyValuePair<double[], double>[]>();
		}

		public KeyValuePair<double[], double>[] Get(int id)
		{
			return _sampleCache.GetOrAdd(id, key =>
			{

				var aSample = Sample();
				var bSample = Sample();
				var result = new KeyValuePair<double[], double>[aSample.Length * bSample.Length];
				var i = 0;
				foreach (var a in aSample)
				{
					foreach (var b in bSample)
					{
						result[i++] = KeyValuePair.New(new double[] { a, b }, _actualFormula(a, b));
					}
				}
				return result;
			}).ToArray();
		}


		double[] Sample(int count = 5, double range = 100)
		{
			var result = new HashSet<double>();
			var offset = RandomUtilities.Random.Next(1000) - 50;

			while (result.Count < count)
			{
				result.Add(RandomUtilities.Random.NextDouble() * range + offset);
			}
			return result.OrderBy(v => v).ToArray();
		}

	}
}