/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */



namespace GeneticAlgorithmPlatform
{
    /// <summary>
    /// Problems define what parameters need to be tested to resolve fitness.
    /// </summary>
    public interface IProblem<TGenome>
		 where TGenome :IGenome
	{
		int ID { get; }

		GenomeTestDelegate<TGenome> TestProcessor { get; }

		IFitness AddToGlobalFitness(IGenomeFitness<TGenome> result);

		int GetSampleCountFor(TGenome genome);
	}

}