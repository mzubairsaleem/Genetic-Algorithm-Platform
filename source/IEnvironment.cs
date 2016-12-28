/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{

    interface IEnvironment<TGenome>
    where TGenome : IGenome
    {
        /**
         * Initiates a cycle of testing with the current populations and problems.
         */
        Task TestAsync(int count = 1);

        /**
         * Spawns a new population. Optionally does so using the source provided.
         * @param populationSize
         * @param source
         */
        Population<TGenome> Spawn(int populationSize, IEnumerable<TGenome> source = null);
        Task<Population<TGenome>> SpawnAsync(int populationSize, IEnumerable<TGenome> source = null);
        
    }

}