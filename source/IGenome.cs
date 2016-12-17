/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;

namespace GeneticAlgorithmPlatform
{
    public interface IGenome /* : ISerializable, IEquatable<IGenome> */
    {
        // This allows for variation testing without constantly overloading.
        uint VariationCountdown { get; set; }

        IGene Root { get; }
        string Hash { get; }
        IEnumerable<IGene> Genes { get; }
    }
}