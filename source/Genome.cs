/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Open;
using Open.Threading;

namespace GeneticAlgorithmPlatform
{

	public abstract class Genome<T> : ModificationSynchronizedBase, IGenome<T>
	where T : IGene
	{
		private T _root;
		public Genome(T root) : base()
		{
			if (root == null)
				throw new ArgumentNullException("root");
			Root = root;
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			base.OnDispose(calledExplicitly);
			_root = default(T);
			_genes = null;
			Interlocked.Exchange(ref _parentCache, null).SmartDispose();
		}

		public T Root
		{
			get
			{
				return _root;
			}
			set
			{
				SetRoot(value);
			}
		}

		public bool SetRoot(T value)
		{
			return Sync.Modifying(
				() => AssertIsLiving() && (_root == null && value != null || _root != null && !_root.Equals(value)),
				() =>
				{
					var old = _root;
					_root = value;
					OnRootChanged(old, value);
					return true;
				});
		}

		protected abstract void OnRootChanged(T oldRoot, T newRoot);

		ConcurrentDictionary<T, IGeneNode<T>> _parentCache;
		public virtual IGeneNode<T> FindParent(T child)
		{
			return _parentCache == null
				? FindParentInternal(child)
				: _parentCache.GetOrAdd(child, key => FindParentInternal(key));
		}
		protected IGeneNode<T> FindParentInternal(T child)
		{
			var rn = _root as IGeneNode<T>;
			if (rn != null)
				return rn.FindParent(child);
			return null;
		}

		Lazy<T[]> _genes;
		public T[] Genes
		{
			get
			{
				var g = _genes;
				return g == null ? GetGenes() : g.Value;
			}
		}

		T[] GetGenes()
		{
			var r = Enumerable.Repeat(_root, 1);
			var rn = _root as IGeneNode<T>;
			return (rn != null ? r.Concat(rn.Descendants) : r).ToArray();
		}

		public string Hash
		{
			get
			{
				return ToString();
			}
		}

		IGene IGenome.Root
		{
			get
			{
				return this.Root;
			}
		}

		IGene[] IGenome.Genes
		{
			get
			{
				return (dynamic)this.Genes;
			}
		}

		public virtual Genome<T> Clone()
		{
			throw new NotImplementedException();
		}

		public bool Equivalent(IGenome other)
		{
			return this == other || _root != null && _root.Equals(other.Root) || Hash == other.Hash;
		}

		protected override void OnFrozen()
		{
			base.OnFrozen();
			Root.Freeze();
			_genes = Lazy.New(() => GetGenes());
			_parentCache = new ConcurrentDictionary<T, IGeneNode<T>>();
		}

		public abstract IGenome NextVariation();
	}

}
