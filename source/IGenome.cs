/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


using System;
using System.Collections.Generic;

namespace GeneticAlgorithmPlatform
{


    public interface IGenome
    {
        // This allows for variation testing without constantly overloading.
        int VariationCountdown { get; set; }

        IGene Root { get; }
        string Hash { get; }
        IEnumerable<IGene> Genes { get; }
    }

    public interface IGenome<T> : IGenome, IEquatable<IGenome<T>> /* : ISerializable */
    where T : IGene
    {
        new T Root { get; }
        new IEnumerable<T> Genes { get; }
    }



}