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
            if (source.Length > 32)
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
            var index = 0;
            var result = double.NaN;
            foreach (var s in source)
            {
                if (index == 0)
                    result = s;
                else
                    result /= s;

                index++;
            }

            return index > 1 ? result : double.NaN;
        }

        public static double QuotientOf(this IEnumerable<double> divisors, double dividend)
        {
            var any = false;
            var result = dividend;
            foreach (var s in divisors)
            {
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



        public static bool IsPrime(this int n)
        {
            var a = System.Math.Abs(n);
            if (a == 2 || a == 3)
                return true;

            if (a % 2 == 0 || a % 3 == 0)
                return false;

            var divisor = 6;
            while (divisor * divisor - 2 * divisor + 1 <= a)
            {

                if (a % (divisor - 1) == 0)
                    return false;

                if (a % (divisor + 1) == 0)
                    return false;

                divisor += 6;

            }

            return true;

        }

        public static bool IsPrime(this double n)
        {
            return (System.Math.Floor(n) == n)
                ? IsPrime((int)n)
                : false;
        }

        public static int NextPrime(this int n)
        {
            if(n==0)
                return 0;

            if (n < 0)
            {
                while (!IsPrime(--n))
                { }
            }
            else
            {
                while (!IsPrime(++n))
                { }
            }

            return n;
        }

        public static int NextPrime(this double a)
        {
            return NextPrime((int)a);
        }

    }


}
