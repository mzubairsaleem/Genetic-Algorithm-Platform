using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    public class Program
    {
        static double SqrtA2B2(params double[] p)
        {
            var a = p[0];
            var b = p[1];
            return Math.Sqrt(a * a + b * b);
        }

        public static void Main(string[] args)
        {
            Task.WaitAny(
                Enumerable.Range(1,1)
                .Select(i=>(new AlgebraBlackBox.Environment(SqrtA2B2)).RunUntilConvergedAsync()).ToArray()
            );
        }

        static void PerfTest()
        {
            var sw = new Stopwatch();
            sw.Start();

            var n = 0;
            for (var j = 0; j < 10; j++)
            {
                for (var i = 0; i < 1000000000; i++)
                {
                    n += i;
                }
            }

            sw.Stop();
            Console.WriteLine("Result: " + n);

            Console.WriteLine("Elapsed Time: " + sw.ElapsedMilliseconds);
        }
    }
}