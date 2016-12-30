/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;

namespace GeneticAlgorithmPlatform
{

    public abstract class Environment<TGenome>
		where TGenome : IGenome
	{
		protected List<IProblem<TGenome>> _problems;
		private IGenomeFactory<TGenome> _genomeFactory;

		protected Environment(IGenomeFactory<TGenome> genomeFactory)
		{
			_genomeFactory = genomeFactory;
			_problems = new List<IProblem<TGenome>>();
		}

		/**
         * Adds a new population to the environment.  Optionally pulling from the source provided.
         */
		public void Spawn()
		{
			var newGenome = _genomeFactory.Generate();
			foreach (var p in _problems) p.Receive(newGenome);
		}
	}


}
