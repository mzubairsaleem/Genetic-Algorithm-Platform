/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    interface IEnvironment<TGenome>
	where TGenome : class, IGenome
	{

		void AddProblem(IProblem<TGenome> problem);

		void SpawnNew(uint count = 1);

		Task<IList<IProblem<TGenome>>> WaitForConverged();
	}

}