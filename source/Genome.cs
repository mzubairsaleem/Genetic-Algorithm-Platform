/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmPlatform
{

    public abstract class Genome<T> : IGenome<T>
    where T : IGene
    {
        private T _root;
        public Genome(T root)
        {
            ResetHash();
            _root = root;
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

        public virtual IGeneNode<T> FindParent(T child)
        {
            if (this._root is IGeneNode<T>)
                return ((IGeneNode<T>)_root).FindParent(child);
            return null;
        }

        public IEnumerable<T> Genes
        {
            get
            {
                var r = Enumerable.Repeat(_root, 1);
                return this._root is IGeneNode<T> ? r.Concat(((IGeneNode<T>)_root).Descendants) : r;
            }
        }

        public virtual string Serialize()
        {
            return this.ToString();
        }

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

        IEnumerable<IGene> IGenome.Genes
        {
            get
            {
                return (IEnumerable<IGene>)this.Genes;
            }
        }

        public virtual void ResetHash()
        {
            _hash = null;
            if (_root != null)
                this._root.ResetToString();
        }

        override public string ToString()
        {
            return Hash;
        }

        public virtual Genome<T> Clone()
        {
            throw new NotImplementedException();
        }

        public bool Equals(IGenome<T> other)
        {
            return this == other || _root != null && _root.Equals(other.Root) || Hash == other.Hash;
        }

    }

}
