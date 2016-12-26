/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

		protected override void OnModified()
		{
			base.OnModified();
			Interlocked.Exchange(ref _variationSpawns, 0);
		}

		protected abstract void OnRootChanged(T oldRoot, T newRoot);

		public virtual IGeneNode<T> FindParent(T child)
		{
			var rn = _root as IGeneNode<T>;
			if (rn != null)
				return rn.FindParent(child);
			return null;
		}

		public IEnumerable<T> Genes
		{
			get
			{
				var r = Enumerable.Repeat(_root, 1);
				var rn = _root as IGeneNode<T>;
				return rn != null ? r.Concat(rn.Descendants) : r;
			}
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

		IEnumerable<IGene> IGenome.Genes
		{
			get
			{
				return (IEnumerable<IGene>)this.Genes;
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
		}

		int _variationSpawns;
		public int VariationSpawn(int allowed)
		{
			var value = _variationSpawns;
			if (value < allowed)
			{
				return Interlocked.Increment(ref _variationSpawns);
			}
			return value;
		}

	}

}
