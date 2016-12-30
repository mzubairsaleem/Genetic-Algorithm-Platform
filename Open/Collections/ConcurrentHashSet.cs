using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using Open.Threading;

namespace Open.Collections
{
    public class ConcurrentHashSet<T> : DisposableBase, IEnumerable<T>
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private readonly HashSet<T> _hashSet = new HashSet<T>();

		#region Implementation of ICollection<T> ...ish
		public bool Add(T item)
		{
			return _lock.Write(() => _hashSet.Add(item));
		}

		public void Clear()
		{
			_lock.Write(() => _hashSet.Clear());
		}

		public bool Contains(T item)
		{
			return _lock.ReadValue( ()=> _hashSet.Contains(item));
		}

		public bool Remove(T item)
		{
			return _lock.WriteValue(() => _hashSet.Remove(item));
		}

		public int Count
		{
			get
			{
				return _lock.ReadValue(() => _hashSet.Count);
			}
		}

		public T[] ToArrayDirect()
		{
			var result = _lock.ReadValue(() => _hashSet.ToArray());
			return result;
		}

		public void Export(HashSet<T> to)
		{
			_lock.Read(() => to.Add(_hashSet));
		}

		#endregion

		#region Dispose
		protected override void OnDispose(bool calledExplicitly)
		{
			_lock.Dispose();
		}

		public HashSet<T> DisposeAndExtract()
		{

			Dispose();
			return _hashSet;
		}
		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)this.ToArrayDirect()).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members


		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)this.GetEnumerator();
		}

		#endregion
	}
}
