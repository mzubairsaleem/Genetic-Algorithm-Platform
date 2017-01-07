/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

// using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    interface IEnvironment<TGenome>
	where TGenome : class, IGenome
	{
		// Once the problem converges the task completes.
		// Task<TGenome> AddProblem(IProblem<TGenome> problem);
	}

}