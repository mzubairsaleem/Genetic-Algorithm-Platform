/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
// using System.Runtime.Serialization;

namespace GeneticAlgorithmPlatform
{

    public interface IGene : ICloneable<IGene>, IEquatable<IGene>
    {
         void ResetToString();
    }

    public interface IGeneNode<T> : IGene, ICollection<T>, ICloneable<IGeneNode<T>> /*,ISerializable*/
    where T : IGene
    {
        IEnumerable<T> Children { get; }
        IEnumerable<T> Descendants { get; }
        IGeneNode<T> FindParent(T child);

        bool Replace(T target, T replacement, bool throwIfNotFound = false);

        void SetAsReadOnly();
    }

    public interface IGeneNode : IGeneNode<IGeneNode>
    {
        new IGeneNode FindParent(IGeneNode child);
    }

}
