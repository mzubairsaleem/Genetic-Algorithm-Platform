using System.Collections.Generic;
using System.Diagnostics;

namespace AlgebraBlackBox
{
	public class Environment : GeneticAlgorithmPlatform.Environment<Genome>
	{

		public Environment(
			Formula actualFormula,
			ushort poolSize,
			uint networkDepth = 3,
			byte nodeSize = 2) : base(new GenomeFactory(), new Problem(actualFormula), poolSize, networkDepth, nodeSize)
		{
		}

		protected override IEnumerable<Genome> Breed(Genome genome)
		{
			// Calling AsReduced will cut down on unnecessary retries of existing formulas.
			var r = genome.AsReduced();
			Debug.Assert(r != null);
			if (r != genome)
			{
				yield return r;
				yield return ((Genome)r.NextVariation()).AsReduced();
				yield return ((Genome)r.NextMutation()).AsReduced();
			}
			foreach (var g in base.Breed(genome))
				yield return g.AsReduced();
		}

	}

}