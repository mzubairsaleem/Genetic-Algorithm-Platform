/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GeneticAlgorithmPlatform
{
	/// <summary>
	/// Problems define what parameters need to be tested to resolve fitness.
	/// Problems execute testing, own the fitness values for their tests and track Genome performance.
	/// </summary>
	public interface IProblem<TGenome>
		 where TGenome : class, IGenome
	{
		int ID { get; }
		
		Task<IGenomeFitness<TGenome>> Top();

		Task<TGenome[]> TopGenomes(int count);

		TGenome[] Convergent { get; }

		Task WaitForConverged();

		IDisposable[] Consume(IGenomeFactory<TGenome> source);

		IDisposable ListenToTopChanges(ITargetBlock<IGenomeFitness<TGenome>> target);

	}
}