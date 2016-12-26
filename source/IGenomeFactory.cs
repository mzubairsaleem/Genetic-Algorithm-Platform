/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    public interface IGenomeFactory<TGenome>
     where TGenome : IGenome
    {
        Task<TGenome> Generate(IEnumerable<TGenome> source = null);
        Task<TGenome> MutateAsync(TGenome source, uint mutations = 1);
        uint MaxGenomeTracking { get; set; }
        string[] PreviousGenomes { get; }
        TGenome GetPrevious(string hash);
        Task TrimPreviousGenomes();
        void Add(TGenome genome);
    }
}
