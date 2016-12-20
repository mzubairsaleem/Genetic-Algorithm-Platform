    using System;
    using System.Diagnostics;

    namespace GeneticAlgorithmPlatform
    {
        public class Program
        {
            public static void Main(string[] args)
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
                Console.WriteLine("Result: "+n);

                Console.WriteLine("Elapsed Time: " + sw.ElapsedMilliseconds);
            }
        }
    }