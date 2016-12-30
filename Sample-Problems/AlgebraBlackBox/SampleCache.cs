using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Open;

namespace AlgebraBlackBox
{
	public sealed class SampleCache
	{
		Formula _actualFormula;
		ConcurrentDictionary<int, ReadOnlyCollection<KeyValuePair<double[], double>>> _sampleCache;

		public SampleCache(Formula actualFormula)
		{
			_actualFormula = actualFormula;
			_sampleCache = new ConcurrentDictionary<int, ReadOnlyCollection<KeyValuePair<double[], double>>>();
		}

		public ReadOnlyCollection<KeyValuePair<double[], double>> Get(int id)
		{
			return _sampleCache.GetOrAdd(id, key =>
			{
				var list = new List<KeyValuePair<double[], double>>();

				var aSample = Sample();
				var bSample = Sample();

				foreach (var a in aSample)
				{
					foreach (var b in bSample)
					{
						list.Add(KeyValuePair.New(new double[] { a, b }, _actualFormula(a, b)));
					}
				}
				return list.AsReadOnly();
			});
		}


		double[] Sample(int count = 5, double range = 100)
		{
			var result = new HashSet<double>();

			while (result.Count < count)
			{
				result.Add(RandomUtilities.Random.NextDouble() * range);
			}
			return result.OrderBy(v => v).ToArray();
		}

	}
}