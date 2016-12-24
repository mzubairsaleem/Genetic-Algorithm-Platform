using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AlgebraBlackBox
{

    // function actualFormula(a:number, b:number):number // Solve for 'c'.
    // {
    // 	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2) + a) + b;
    // }

    public class Environment : GeneticAlgorithmPlatform.Environment<Genome>
    {

        public Environment(Formula actualFormula) : base(new GenomeFactory())
        {
            this._problems
                .Add(new Problem(actualFormula));

            this.MaxPopulations = 20;
            this.PopulationSize = 100;
        }

        protected override async Task _onExecute()
        {
            await base._onExecute();

            var p = this._populations
                .SelectMany(s => s.Values)
                .OrderBy(g => g.Hash.Length)
                .GroupBy(g => g.CachedToStringReduced)
                .Select(g => g.First());

            var top = _problems
                .Select(r => r.Rank(p).Select(g =>
                  {
                      var alpha = g.ToAlphaParameters();
                      var alphaRed = g.ToAlphaParameters(true);
                      var suffix = alpha == alphaRed ? "" : (" => " + g.ToAlphaParameters(true));
                      var f = r.GetFitnessFor(g);
                      Debug.Assert(f!=null);
                      return new
                      {
                          Label = String.Format(
                              "{0}: ({1} samples) [{2}]",
                              alpha + suffix,
                              f.SampleCount,
                              String.Join(", ",
                                f.Scores.Select(v => v.ToString()).ToArray())),
                          Gene = g
                      };
                  })
                )
                .Weave()
                .Take(_problems.Count)
                .Memoize();

            // this.state = "Top Genome: "+topOutput.replace(": ",":\n\t"); // For display elsewhere.
            Console.WriteLine("Top:");
            foreach(var label in top.Select(s => s.Label))
            {
                Console.Write("\t");
                Console.WriteLine(label);
            }

            var c = _problems.SelectMany(pr => pr.Convergent).ToArray();
            if (c.Length != 0) Console.WriteLine("\nConvergent:", c.Select(
                   g => g.ToAlphaParameters(true)));


            if (_problems.Count(pr => pr.Convergent.Any()) < this._problems.Count)
            {
                var n = this._populations.Last.Value;
                n.Add(top
                    .Select(g => g.Gene)
                    .Where(g => g.Root.IsReducible() && g.Root.AsReduced() != g.Root)
                    .Select(g =>
                    {
                        var m = g.Clone();
                        m.Root = g.Root.AsReduced();
                        return m;//.setAsReadOnly();
                    }));

                // this.start();
            }

            Console.WriteLine("");


        }


    }



}
