using System.Collections.Generic;
using Open.Threading;

namespace Open.Collections
{
    public class ConcurrentHashSet<T> : ConcurrentCollectionBase<T, HashSet<T>>, ISet<T>
	{

		public ConcurrentHashSet() : base(new HashSet<T>()) { }
		public ConcurrentHashSet(IEnumerable<T> collection) : base(new HashSet<T>(collection)) { }
		public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(new HashSet<T>(collection, comparer)) { }

		public new bool Add(T item)
		{
			return Sync.Write(() => Source.Add(item));
		}

        public void ExceptWith(IEnumerable<T> other)
        {
		   Sync.Write(() => Source.ExceptWith(other));
        }

        public void IntersectWith(IEnumerable<T> other)
        {
			Sync.Write(() =>Source.IntersectWith(other));
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
			return Sync.ReadValue(() => Source.IsProperSubsetOf(other));
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
			return Sync.ReadValue(() => Source.IsProperSupersetOf(other));
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
			return Sync.ReadValue(() => Source.IsSubsetOf(other));
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
			return Sync.ReadValue(() => Source.IsSupersetOf(other));
        }

        public bool Overlaps(IEnumerable<T> other)
        {
			return Sync.ReadValue(() => Source.Overlaps(other));
        }

        public bool SetEquals(IEnumerable<T> other)
        {
			return Sync.ReadValue(() => Source.SetEquals(other));
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
			Sync.Write(() => Source.SymmetricExceptWith(other));
        }

        public void UnionWith(IEnumerable<T> other)
        {
			Sync.Write(() => Source.UnionWith(other));
        }

    }
}
