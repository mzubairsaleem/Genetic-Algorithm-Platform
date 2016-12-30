/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    public interface IProblem<TGenome>
         where TGenome : IGenome
    {
        TGenome TakeNextTopGenome();
        TGenome PeekNextTopGenome();
        void Receive(TGenome genome);
        Task<TGenome> WaitForConverged();
    }
}