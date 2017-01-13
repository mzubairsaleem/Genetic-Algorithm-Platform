/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */



namespace GeneticAlgorithmPlatform
{


    public interface IGenome
    {
        IGene Root { get; }
        string Hash { get; }
        IGene[] Genes { get; }


        IGenome NextVariation();
        IGenome NextMutation();

        /*
         * Should prevent further modifications to the genome.
         */
        void Freeze();

        // True if frozen.
        bool IsReadOnly {get;}

        bool Equivalent(IGenome other);
     
    }

    public interface IGenome<T> : IGenome /* : ISerializable */
    where T : IGene
    {
        new T Root { get; }
        new T[] Genes { get; }
    }



}