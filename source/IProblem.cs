/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GeneticAlgorithmPlatform
{
    public interface IProblem<TGenome>
         where TGenome : IGenome
    {
        TGenome TakeNextTop();
        void GetConvergent(BufferBlock<TGenome> queue);
        ICollection<TGenome> Convergent { get; }
        Fitness GetFitnessFor(TGenome genome, bool createIfMissing = false);
        // Due to the complexity of potential fitness values, this provides a single place to rank a population.
        IEnumerable<TGenome> Rank(IEnumerable<TGenome> population);
        // Some outlying survivors may be tied in their fitness and there needs to be a way to retain them without a hard trim.

        Task Test(IEnumerable<TGenome> population, int count = 1);

        Task<Fitness> Test(TGenome genome);
    }
}