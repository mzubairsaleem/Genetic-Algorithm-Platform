using System.Collections.Generic;
using System.Diagnostics;

namespace AlgebraBlackBox
{

	// function actualFormula(a:number, b:number):number // Solve for 'c'.
	// {
	// 	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2) + a) + b;
	// }

	public class Environment : GeneticAlgorithmPlatform.Environment<Genome>
	{

		public Environment(Formula actualFormula, ushort poolSize) : base(new GenomeFactory(), new Problem(actualFormula), poolSize)
		{
		}

		protected override IEnumerable<Genome> Breed(Genome genome)
		{
			foreach (var g in base.Breed(genome))
				yield return g;
			var r = genome.AsReduced();
			Debug.Assert(r != null);
			if (r != genome)
				yield return r;
		}

	}

}