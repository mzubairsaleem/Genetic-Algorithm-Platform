/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmPlatform
{

    public static class Triangular
    {

        public static uint Forward(uint n)
        {
            return n * (n + 1) / 2;
        }


        public static uint Reverse(uint n)
        {
            return (uint)(Math.Sqrt(8 * n + 1) - 1) / 2;
        }

        public static class Disperse
        {

            /**
             * Increases the number an element based on it's index.
             * @param source
             * @returns {Enumerable<T>}
             */
            public static IEnumerable<T> Increasing<T>(IEnumerable<T> source)
            {
                return source
                    .SelectMany(
                        (c, i) => Enumerable.Repeat(c, i + 1));
            }

            /**
             * Increases the count of each element for each index.
             * @param source
             * @returns {Enumerable<T>}
             */
            public static IEnumerable<T> Decreasing<T>(IEnumerable<T> source)
            {
                var s = source.ToArray();
                return s.SelectMany(
                        (c, i) => s.Take(i + 1));
            }
        }
    }

}
