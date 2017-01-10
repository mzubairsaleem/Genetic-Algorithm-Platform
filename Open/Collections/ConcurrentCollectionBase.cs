using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using Open.Threading;

namespace Open.Collections
{
	public abstract class ConcurrentCollectionBase<T, TCollection> : DisposableBase, ICollection<T>
		where TCollection : class, ICollection<T>
	{
		protected ReaderWriterLockSlim Sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion); // Support recursion for read -> write locks.
		protected TCollection Source;

		protected ConcurrentCollectionBase(TCollection source)
		{
			if(source==null)
				throw new ArgumentNullException("source");
			Source = source;
		}

		#region Implementation of ICollection<T>
		public void Add(T item)
		{
			Sync.Write(() => Source.Add(item));
		}

		public void Clear()
		{
			Sync.Write(() => Source.Clear());
		}

		public bool Contains(T item)
		{
			return Sync.ReadValue(() => Source.Contains(item));
		}

		public bool Remove(T item)
		{
			bool result = false;
			Sync.ReadWriteConditionalOptimized(
				lockType => result = Source.Contains(item),
				() => result = Source.Remove(item));
			return result;
		}

		public int Count
		{
			get
			{
				return Sync.ReadValue(() => Source.Count);
			}
		}

        public bool IsReadOnly
        {
            get
            {
                return Source.IsReadOnly;
            }
        }

        public T[] ToArrayDirect()
		{
			var result = Sync.ReadValue(() => Source.ToArray());
			return result;
		}

		public void Export(ICollection<T> to)
		{
			Sync.Read(() => to.Add(Source));
		}

		#endregion

		#region Dispose
		protected override void OnDispose(bool calledExplicitly)
		{
			Interlocked.Exchange(ref Sync, null).Dispose();
			Interlocked.Exchange(ref Source, null).SmartDispose();
		}

		public TCollection DisposeAndExtract()
		{
			var s = Source;
			Source = null;
			Dispose();
			return s;
		}
		#endregion


		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)this.ToArrayDirect()).GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this.ToArrayDirect()).GetEnumerator();
		}

        public void CopyTo(T[] array, int arrayIndex)
        {
			Sync.Read(() => Source.CopyTo(array, arrayIndex));
        }

    }
}
