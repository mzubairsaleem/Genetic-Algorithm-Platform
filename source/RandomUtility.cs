/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmPlatform
{

    public static class RandomUtil
    {
		static Lazy<Random> R = new Lazy<Random>(()=>new Random());
		public static T RandomSelectOne<T>(this IList<T> source)
		{
			return source[R.Value.Next(source.Count)];
		}
		public static T RandomSelectOne<T>(this T[] source)
		{
			return source[R.Value.Next(source.Length)];
		}
		public static T RandomSelectOne<T>(this ICollection<T> source)
		{
			return source.Skip(R.Value.Next(source.Count)).First();
		}
		public static T RandomSelectOne<T>(this IEnumerable<T> source)
		{
			return source.Skip(R.Value.Next(source.Count())).First();
		}

        public static int NextRandomIntegerExcluding(
            int range,
            IEnumerable<int> excluding)
        {
            if (range < 0)
                throw new ArgumentOutOfRangeException("range", range, "Must be a number greater than zero.");
            var r = new List<int>();

            var excludeSet = new HashSet<int>(excluding);

            for (var i = 0; i < range; ++i)
            {
                if (!excludeSet.Contains(i)) r.Add(i);
            }

            return r.RandomSelectOne();
        }

    }

}
