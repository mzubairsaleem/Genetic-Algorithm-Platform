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

		TGenome GenerateOne(TGenome source);

		TGenome GenerateOne(IEnumerable<TGenome> source = null);

		IEnumerable<TGenome> Generate(TGenome source);

		IEnumerable<TGenome> Generate(IEnumerable<TGenome> source = null);

		bool AttemptNewMutation(TGenome source, out TGenome mutation, byte triesPerMutationLevel = 5, byte maxMutations = 3);

		bool AttemptNewMutation(IEnumerable<TGenome> source, out TGenome mutation, byte triesPerMutationLevel = 5, byte maxMutations = 3);

		IEnumerable<TGenome> Mutate(TGenome source);

		bool AttemptNewCrossover(TGenome a, TGenome b, out TGenome[] offspring, byte maxAttempts = 3);
		
		bool AttemptNewCrossover(IEnumerable<TGenome> source, out TGenome[] offspring, byte maxAttemptsPerCombination = 3);
	}
}
