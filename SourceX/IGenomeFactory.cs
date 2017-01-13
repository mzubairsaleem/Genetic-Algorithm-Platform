/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


using System.Collections.Generic;

namespace GeneticAlgorithmPlatform
{
	public interface IGenomeFactory<TGenome>
	 where TGenome : class, IGenome
	{
		/*
		 * Note that the input collections are all arrays for some important reasons:
		 * 1) The underlying computation does not want an enumerable that can change in size while selecing for a genome.
		 * 2) In order to select at random, the length of the collection must be known.
		 * 3) Forces the person using this class to smartly think about how to provide the array.
		 */

		TGenome GenerateOne(TGenome source);

		TGenome GenerateOne(TGenome[] source = null);

		IEnumerable<TGenome> Generate(TGenome source);

		IEnumerable<TGenome> Generate(TGenome[] source = null);

		bool AttemptNewMutation(TGenome source, out TGenome mutation, byte triesPerMutationLevel = 5, byte maxMutations = 3);

		bool AttemptNewMutation(TGenome[] source, out TGenome mutation, byte triesPerMutationLevel = 5, byte maxMutations = 3);

		IEnumerable<TGenome> Mutate(TGenome source);

		TGenome[] AttemptNewCrossover(TGenome a, TGenome b, byte maxAttempts = 3);
		
		TGenome[] AttemptNewCrossover(TGenome primary, TGenome[] others, byte maxAttemptsPerCombination = 3);

		TGenome[] AttemptNewCrossover(TGenome[] source, byte maxAttemptsPerCombination = 3);
	}
}
