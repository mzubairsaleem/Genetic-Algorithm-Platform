/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using Open.Threading;
using Open.Collections;

namespace GeneticAlgorithmPlatform
{

	public abstract class GeneBase : IGene
	{

		public ModificationSynchronizer Sync
		{
			get;
			private set;
		}

		public int Version
		{
			get
			{
				return Sync.Version;
			}
		}

		protected virtual ModificationSynchronizer InitSync()
		{
			return new ModificationSynchronizer();
		}


		public GeneBase()
		{
			OnModified();
			Sync = InitSync();
			Sync.Modified += OnModified;
		}

		Lazy<string> _toString;
		public void ResetToString()
		{
			if (_toString == null || _toString.IsValueCreated)
				_toString = Lazy.New(ToStringInternal);
		}

		protected void OnModified(object source = null, EventArgs e = null)
		{
			OnModified();
		}

		protected virtual void OnModified()
		{
			ResetToString();
		}

		protected abstract string ToStringInternal();

		public sealed override string ToString()
		{
			return _toString.Value;
		}

		public virtual string Serialize()
		{
			return ToString();
		}

		public virtual IGene Clone()
		{
			return CloneInternal();
		}

		protected abstract IGene CloneInternal();

		public virtual bool Equals(IGene other)
		{
			return this == other || this.ToString() == other.ToString();
		}

	}

	public abstract class GeneBase<T> : GeneBase, IGeneNode<T>
	where T : IGene
	{

		override protected ModificationSynchronizer InitSync()
		{
			_children = new ThreadSafeTrackedList<T>();
			return _children.Sync;
		}

		protected ThreadSafeTrackedList<T> _children;

		public IEnumerable<T> Children
		{
			get
			{
				// Since we can't know if changes occur in our children, we have to return the uncached value until it is set to read-only.
				return _children;
			}
		}

		IEnumerable<IGeneNode<T>> _getChildNodes;
		public IEnumerable<IGeneNode<T>> ChildNodes
		{
			get
			{
				return LazyInitializer.EnsureInitialized(ref _getChildNodes,
				() => _children.Where(c => c is IGeneNode<T>).Cast<IGeneNode<T>>());
			}
		}

		public int Count
		{
			get
			{
				return _children.Count;
			}
		}


		IEnumerable<T> _getDescendants;
		protected IEnumerable<T> GetDescendants()
		{
			return LazyInitializer.EnsureInitialized(ref _getDescendants,
				() => _children.Concat(ChildNodes.SelectMany(s => s.Descendants)));
		}

		Lazy<T[]> _descendants;
		public IEnumerable<T> Descendants
		{
			get
			{
				// Since we can't know if changes occur in our children, we have to return the uncached value until it is set to read-only.
				return this.IsReadOnly ? _descendants.Value : GetDescendants();
			}
		}

		bool _isReadOnly = false;
		public bool IsReadOnly
		{
			get
			{
				return _isReadOnly;
			}
		}

		public void SetAsReadOnly()
		{
			_isReadOnly = true;
			foreach (var c in _children.Where(c => c is IGeneNode).Cast<IGeneNode>())
				c.SetAsReadOnly();
		}

		public virtual void Add(T item)
		{
			_children.Add(item);
		}

		public void AddThese(IEnumerable<T> items)
		{
			_children.Add(items);
		}

		public void Clear()
		{
			_children.Clear();
		}

		public bool Contains(T item)
		{
			return _children.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_children.CopyTo(array, arrayIndex);
		}

		public IGeneNode<T> FindParent(T child)
		{
			return Sync.Reading(() =>
			{
				if (Count == 0) return null;
				if (_children.IndexOf(child) != -1) return this;

				foreach (var c in ChildNodes)
				{
					var p = c.FindParent(child);
					if (p != null) return p;
				}

				return null;
			});

		}

		public bool Remove(T item)
		{
			return _children.Remove(item);
		}


		public IEnumerator<T> GetEnumerator()
		{
			return _children.GetEnumerator();
		}

		public bool Replace(T item, T replacement, bool throwIfNotFound = false)
		{
			return _children.Replace(item, replacement, throwIfNotFound);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _children.GetEnumerator();
		}


		protected override void OnModified()
		{
			base.OnModified();
			if (_descendants == null || _descendants.IsValueCreated)
				_descendants = Lazy.New(() => GetDescendants().ToArray());			
		}


		public new IGeneNode<T> Clone()
		{
			return Sync.Reading(() => (IGeneNode<T>)CloneInternal());
		}


	}

}