/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


using System.Threading.Tasks.Dataflow;

namespace GeneticAlgorithmPlatform
{
    public interface IGenomeFactory<TGenome>
     where TGenome : class, IGenome
    {
        void Generate(uint count);

        void LinkReception(ITargetBlock<TGenome> block);

        void LinkMutation(ISourceBlock<TGenome> block);

        // TGenome Generate(IEnumerable<TGenome> source = null);
        // Task<TGenome> GenerateAsync(IEnumerable<TGenome> source = null);
        // TGenome Mutate(TGenome source, uint mutations = 1);
        // Task<TGenome> MutateAsync(TGenome source, uint mutations = 1);
        // uint MaxGenomeTracking { get; set; }
        // string[] PreviousGenomes { get; }
        // TGenome GetPrevious(string hash);
        // Task TrimPreviousGenomes();
        // void Add(TGenome genome);
    }
}
