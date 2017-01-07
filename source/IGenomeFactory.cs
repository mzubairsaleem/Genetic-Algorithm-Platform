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

		TGenome Generate(TGenome source);

		TGenome Generate(IEnumerable<TGenome> source = null);

		bool AttemptNewMutation(TGenome source, out TGenome mutation, int triesPerMutation = 10);

		bool AttemptNewMutation(IEnumerable<TGenome> source, out TGenome mutation, int triesPerMutation = 10);

		IEnumerable<TGenome> Generator();
		IEnumerable<TGenome> Mutator(TGenome source);

		// TGenome Generate(IEnumerable<TGenome> source = null);
		// Task<TGenome> GenerateAsync(IEnumerable<TGenome> source = null);
		// Task<TGenome> MutateAsync(TGenome source, uint mutations = 1);
		// uint MaxGenomeTracking { get; set; }
		// string[] PreviousGenomes { get; }
		// TGenome GetPrevious(string hash);
		// Task TrimPreviousGenomes();
		// void Add(TGenome genome);
	}
}
