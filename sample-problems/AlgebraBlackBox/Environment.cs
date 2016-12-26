using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Open.Collections;

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
		}

		protected override async Task _onExecute()
		{
			await base._onExecute();

			var p = this._populations
				//.AsParallel()
				.SelectMany(s => s.Values)
				.OrderBy(g => g.Hash.Length) // Pick the most reduced version.
				.GroupBy(g => g.AsReduced().ToString())
				.Select(g => g.First());

			using (var top = _problems
				.Select(r => r.Rank(p).Select(g =>
				  {
					  var alpha = g.ToAlphaParameters();
					  var alphaRed = g.ToAlphaParameters(true);
					  var suffix = alpha == alphaRed ? "" : (" => " + g.ToAlphaParameters(true));
					  var f = r.GetFitnessFor(g);
					  Debug.Assert(f != null);
					  return new
					  {
						  Label = String.Format(
							  "{0}: ({1} samples) [{2}]",
							  alpha + suffix,
							  f.SampleCount,
							  String.Join(", ",
								f.Scores.Select(v => v.ToString()).ToArray())),
						  Genome = g
					  };
				  })
				)
				.Weave()
				.Take(_problems.Count)
				.Memoize())
			{

				// this.state = "Top Genome: "+topOutput.replace(": ",":\n\t"); // For display elsewhere.
				var topDisplay = top.Select(s => s.Label).ToArray();
				Console.WriteLine("Top:");
				foreach (var label in topDisplay)
				{
					Console.Write("\t");
					Console.WriteLine(label);
				}

				var c = _problems.SelectMany(pr => pr.Convergent).ToArray();
				if (c.Length != 0)
					Console.WriteLine("Convergent:"
						+ c.Select(g => g.ToAlphaParameters(true)));


				if (_problems.Count(pr => pr.Convergent.Any()) < this._problems.Count)
				{
					var n = this._populations.Last.Value;
					n.Add(top
						.Select(g => g.Genome)
						.Where(g => g.IsReducible)
						.Select(g => g.AsReduced())
					);
				}
				else
					Converged = true;
			}

			Console.WriteLine("");

		}


	}



}
