/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    public interface IGenomeFactory<TGenome>
     where TGenome : IGenome
    {
        Task<TGenome[]> GenerateVariations(TGenome source);
        Task<TGenome> Generate(TGenome[] source = null);
        Task<TGenome> Mutate(TGenome source, uint mutations = 1);
        uint MaxGenomeTracking { get; set; }
        string[] PreviousGenomes { get; }
        TGenome GetPrevious(string hash);
        Task TrimPreviousGenomes();
        void Add(TGenome genome);
    }
}
