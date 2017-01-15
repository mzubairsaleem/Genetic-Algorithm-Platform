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
		IGene[] Genes { get; }


		IGenome NextMutation();

		IGenome NextVariation();
		IReadOnlyList<IGenome> Variations { get; }
        
		/*
         * Should prevent further modifications to the genome.
         */
		void Freeze();

		// True if frozen.
		bool IsReadOnly { get; }

		bool Equivalent(IGenome other);

	}

	public interface IGenome<T> : IGenome /* : ISerializable */
	where T : IGene
	{
		new T Root { get; }
		new T[] Genes { get; }
	}



}