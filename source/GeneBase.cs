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
using Open;

namespace GeneticAlgorithmPlatform
{

	public abstract class GeneBase : ModificationSynchronizedBase, IGene
	{

		Lazy<string> _toString;
		public virtual void ResetToString()
		{
			if (_toString == null || _toString.IsValueCreated)
				_toString = Lazy.New(ToStringInternal);
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			base.OnDispose(calledExplicitly);
			_toString = null;
		}

		protected override void OnModified()
		{
			base.OnModified();
			ResetToString();
		}

		protected abstract string ToStringInternal();

		public sealed override string ToString()
		{
			return _toString == null ? base.ToString() : _toString.Value;
		}

		public virtual string Serialize()
		{
			return ToString();
		}

		public virtual IGene Clone()
		{
			return Sync.Reading(() => CloneInternal());
		}

		protected abstract IGene CloneInternal();

		public virtual bool Equivalent(IGene other)
		{
			return this == other || other != null && this.ToString() == other.ToString();
		}

		protected override void OnFrozen()
		{
			base.OnFrozen();
			ResetToString();
		}

	}

	public abstract class GeneBase<T> : GeneBase, IGeneNode<T>
	where T : IGene
	{

		protected override void OnDispose(bool calledExplicitly)
		{
			base.OnDispose(calledExplicitly);
			var c = _children as IDisposable;
			_children = null;
			c.SmartDispose();
			_getChildNodes = null;
			_getDescendants = null;
		}

		override protected ModificationSynchronizer InitSync(object sync = null)
		{
			ModificationSynchronizer s;
			_children = new ThreadSafeTrackedList<T>(out s);
			return s;
		}

		private ThreadSafeTrackedList<T> _children;

		public IEnumerable<T> Children
		{
			get
			{
				return GetChildren();
			}
		}

		protected ThreadSafeTrackedList<T> GetChildren()
		{
			// Since we can't know if changes occur in our children, we have to return the uncached value until it is set to read-only.
			var c = _children;
			AssertIsLiving();
			return c;
		}

		IEnumerable<IGeneNode<T>> _getChildNodes;
		public IEnumerable<IGeneNode<T>> ChildNodes
		{
			get
			{
				var children = GetChildren();
				return LazyInitializer.EnsureInitialized(ref _getChildNodes,
					() => children.Where(c => c is IGeneNode<T>).Cast<IGeneNode<T>>());
			}
		}

		public int Count
		{
			get
			{
				return GetChildren().Count;
			}
		}


		IEnumerable<T> _getDescendants;
		protected IEnumerable<T> GetDescendants()
		{
			var children = GetChildren();
			return LazyInitializer.EnsureInitialized(ref _getDescendants,
				() => children.Concat(ChildNodes.SelectMany(s => s.Descendants)));
		}

		Lazy<T[]> _descendants;
		public IEnumerable<T> Descendants
		{
			get
			{
				var d = _descendants;
				AssertIsLiving();
				// Since we can't know if changes occur in our children, we have to return the uncached value until it is set to read-only.
				return d==null ? GetDescendants() : d.Value;
			}
		}

		public virtual void Add(T item)
		{
			GetChildren().Add(item);
		}

		public void Add(T item, T item2, params T[] items)
		{
			GetChildren().AddThese(new T[] { item, item2 }.Concat(items));
		}

		public void AddThese(IEnumerable<T> items)
		{
			GetChildren().AddThese(items);
		}

		public void Clear()
		{
			GetChildren().Clear();
		}

		public bool Contains(T item)
		{
			return GetChildren().Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			GetChildren().CopyTo(array, arrayIndex);
		}

		public IGeneNode<T> FindParent(T child)
		{
			return Sync.Reading(() =>
			{
				var children = GetChildren();
				if (children.Count == 0) return null;
				if (children.IndexOf(child) != -1) return this;

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
			return GetChildren().Remove(item);
		}


		public IEnumerator<T> GetEnumerator()
		{
			return GetChildren().GetEnumerator();
		}

		public bool ReplaceChild(T item, T replacement, bool throwIfNotFound = false)
		{
			return GetChildren().Replace(item, replacement, throwIfNotFound);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetChildren().GetEnumerator();
		}

		public new IGeneNode<T> Clone()
		{
			return Sync.Reading(() => (IGeneNode<T>)CloneInternal());
		}

		protected override void OnFrozen()
		{
			base.OnFrozen();
			var children = GetChildren();
			children.Freeze();
			foreach (var child in children)
				child.Freeze();

			_descendants = Lazy.New(() => GetDescendants().ToArray());
		}


	}

}