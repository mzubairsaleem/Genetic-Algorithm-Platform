/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    public interface IPopulation<TGenome> : ICollection<TGenome>
     where TGenome : IGenome

    {
        Task Populate(uint count, TGenome[] source = null);
    }

}
