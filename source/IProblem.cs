/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    public interface IProblem<TGenome>
         where TGenome : IGenome
    {
        ICollection<TGenome> Convergent { get; }
        Fitness GetFitnessFor(TGenome genome, bool createIfMissing = false);
        // Due to the complexity of potential fitness values, this provides a single place to rank a population.
        IEnumerable<TGenome> Rank(IEnumerable<TGenome> population);
        // Some outlying survivors may be tied in their fitness and there needs to be a way to retain them without a hard trim.
        //IEnumerable<TGenome> RankAndReduce(IEnumerable<TGenome> population, int targetMaxPopulation);
        Task Test(Population<TGenome> population, int count = 1);
    }
}