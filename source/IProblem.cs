/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
	/// <summary>
	/// Problems define what parameters need to be tested to resolve fitness.
	/// Problems execute testing, own the fitness values for their tests and track Genome performance.
	/// </summary>
	public interface IProblem<TGenome>
		 where TGenome : class, IGenome
	{

		TGenome TopGenome();

		TGenome[] TopGenomes(int count);

		TGenome[] Convergent { get; }

		Task WaitForConverged();

		void Consume(IGenomeFactory<TGenome> source);

	}
}