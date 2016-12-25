using System;
using System.Collections.Generic;
using System.Linq;

namespace Open.Arithmetic
{
    public static class Statistical
    {
        public static double Variance(this IEnumerable<double> source)
        {
            if (source == null)
                throw new NullReferenceException();

            return source.Select(s => s * s).Average() - System.Math.Pow(source.Average(), 2);
        }

        public static IEnumerable<double> Products(this IEnumerable<double> source, IEnumerable<double> target)
        {
            if (source == null)
                throw new NullReferenceException();
            if (target == null)
                throw new ArgumentNullException();

            var sourceEnumerator = source.GetEnumerator();
            var targetEnumerator = target.GetEnumerator();

            while (true)
            {
                bool sv = sourceEnumerator.MoveNext();
                bool tv = targetEnumerator.MoveNext();

                if (sv != tv)
                    throw new Exception("Products: source and target enumerations have different counts.");

                if (!sv || !tv)
                    break;

                yield return sourceEnumerator.Current * targetEnumerator.Current;
            }
        }

        public static double Covariance(this IEnumerable<double> source, IEnumerable<double> target)
        {
            if (source == null)
                throw new NullReferenceException();
            if (target == null)
                throw new ArgumentNullException();

            return source.Products(target).Average() - source.Average() * target.Average();
        }

        public static double Correlation(double covariance, double sourceVariance, double targetVariance)
        {
            return covariance / System.Math.Sqrt(sourceVariance * targetVariance);
        }

        public static double Correlation(double covariance, IEnumerable<double> source, IEnumerable<double> target)
        {
            if (source == null)
                throw new NullReferenceException();
            if (target == null)
                throw new ArgumentNullException();

            return Correlation(covariance, source.Variance(), target.Variance());
        }

        public static double Correlation(this IEnumerable<double> source, IEnumerable<double> target)
        {
            if (source == null)
                throw new NullReferenceException();
            if (target == null)
                throw new ArgumentNullException();

            return Correlation(source.Covariance(target), source, target);
        }
    }
}
