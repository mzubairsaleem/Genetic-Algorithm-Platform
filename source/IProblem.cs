/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */



using System.Collections.Generic;

namespace GeneticAlgorithmPlatform
{
	/// <summary>
	/// Problems define what parameters need to be tested to resolve fitness.
	/// </summary>
	public interface IProblem<TGenome>
		 where TGenome : IGenome
	{
		int ID { get; }

		GenomeTestDelegate<TGenome> TestProcessor { get; }

		void AddToGlobalFitness<T>(IEnumerable<T> results) where T : IGenomeFitness<TGenome>;
		IFitness AddToGlobalFitness(IGenomeFitness<TGenome> result);
		IFitness AddToGlobalFitness(TGenome genome, IFitness result);

		GenomeFitness<TGenome, Fitness>? GetFitnessFor(TGenome genome, bool ensureSourceGenome = false);
		IEnumerable<IGenomeFitness<TGenome, Fitness>> GetFitnessFor(IEnumerable<TGenome> genome, bool ensureSourceGenomes = false);

		int GetSampleCountFor(TGenome genome);
	}

}