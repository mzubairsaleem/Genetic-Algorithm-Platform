/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


using System.Collections.Generic;

namespace GeneticAlgorithmPlatform
{


    public interface IGenome
    {
        IGene Root { get; }
        string Hash { get; }
        IEnumerable<IGene> Genes { get; }

        int VariationSpawn(int allowed);

        /*
         * Should prevent further modifications to the genome.
         */
        void Freeze();

        bool Equivalent(IGenome other);
     
    }

    public interface IGenome<T> : IGenome /* : ISerializable */
    where T : IGene
    {
        new T Root { get; }
        new IEnumerable<T> Genes { get; }
    }



}