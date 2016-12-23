using System;
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
                      var red = g.Root.AsReduced();
                      var alpha = g.ToAlphaParameters();
                      var suffix = "";
                      if (red != g.Root)
                          suffix = " => " + g.ToAlphaParameters(true);
                      var f = r.GetFitnessFor(g);
                      return new
                      {
                          Label = String.Format(
                              "{0}: ({1} samples) [{2}]",
                              alpha + suffix,
                              f.SampleCount,
                              String.Join(", ", f.Scores.Select(v => v.ToString()).ToArray())),
                          Gene = g
                      };
                  })
                )
                .Weave()
                .Take(_problems.Count)
                .Memoize();

            var c = _problems.SelectMany(pr => pr.Convergent).ToArray();
            var topOutput = "\n\t" + String.Join("\n\t", top.Select(s => s.Label).ToArray());
            // this.state = "Top Genome: "+topOutput.replace(": ",":\n\t"); // For display elsewhere.
            Console.WriteLine("Top:", topOutput);
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
