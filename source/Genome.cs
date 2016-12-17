/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmPlatform
{

    public abstract class Genome<T> : IGenome
    where T : IGene
    {
        private T _root;
        Genome()
        {
            _hash = null;
            VariationCountdown = 0;
        }

        public int VariationCountdown { get; set; }

        public T Root
        {
            get
            {
                return _root;
            }
            set
            {
                if (_root == null && value == null || _root != null && !_root.Equals(value))
                {
                    ResetHash();
                    _root = value;
                }
            }
        }

        public IGene FindParent(IGene child)
        {
            return _root.FindParent(child);
        }

        public IEnumerable<IGene> Genes
        {
            get
            {
                return Enumerable.Repeat<IGene>(_root, 1)
                    .Concat(_root.Descendants);
            }
        }

        public abstract string Serialize();

        private string _hash;
        public string Hash
        {
            get
            {
                return _hash ?? (_hash = Serialize());
            }
        }

        IGene IGenome.Root
        {
            get
            {
                return this.Root;
            }
        }

        public void ResetHash()
        {
            _hash = null;
            if (_root != null)
                this._root.resetToString();
        }

        override public string ToString()
        {
            return Hash;
        }

        public abstract Genome<T> Clone();

        public bool Equals(Genome<T> other)
        {
            return this == other || _root != null && _root.Equals(other._root) || Hash == other.Hash;

        }
    }

}
