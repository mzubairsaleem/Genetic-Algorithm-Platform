/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{

	public interface IGene : ICloneable<IGene>
	{
		/*
         * Should prevent further modifications to the genome.
         */
		void Freeze();
		bool Equivalent(IGene other);
	}

	public interface ICalculableGene<T> : IGene
	{
		// Also considered 'Modifier'.
		T Multiple { get; set; }

		bool SetMultiple(T value);

		T Calculate(IReadOnlyList<T> values);
		Task<T> CalculateAsync(IReadOnlyList<T> values);

	}

	public interface IGeneNode<T> : IGene, ICollection<T>, ICloneable<IGeneNode<T>> /*,ISerializable*/
	where T : IGene
	{
		IEnumerable<T> Children { get; }
		IEnumerable<T> Descendants { get; }
		IGeneNode<T> FindParent(T child);

		bool ReplaceChild(T target, T replacement, bool throwIfNotFound = false);

	}

	public interface IGeneNode : IGeneNode<IGeneNode>
	{
		new IGeneNode FindParent(IGeneNode child);
	}

}
