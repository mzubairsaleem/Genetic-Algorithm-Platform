/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Open.Threading;

namespace Open.Collections
{
	public class ThreadSafeTrackedList<T> : DisposableBase, IList<T>
	{
		List<T> _source = new List<T>();
		public int Version
		{
			get {
      			AssertIsLiving();
                return Sync.Version;
            }
		}

		public ModificationSynchronizer Sync
		{
			get;
			private set;
		}
        bool _syncOwned;

		public ThreadSafeTrackedList(ModificationSynchronizer sync = null)
		{
			if (sync == null)
			{
				sync = InitNewSync();
			}
			Sync = sync;
		}

		protected virtual ModificationSynchronizer InitNewSync(ReaderWriterLockSlim sync = null)
		{
            _syncOwned = true;
			return new ModificationSynchronizer(sync);
		}

        protected override void OnDispose(bool calledExplicitly)
        {
			var s = Sync;
			Sync = null;
			if(_syncOwned) s.Dispose();
            _source = null;
        }

		public T this[int index]
		{
			get
			{
                AssertIsLiving();
				return Sync.Reading(() => _source[index]);
			}

			set
			{
				SetValue(index, value);
			}
		}

		public bool SetValue(int index, T value)
		{
            AssertIsLiving();
			return Sync.Modifying(() => SetValueInternal(index, value));
		}

		private bool SetValueInternal(int index, T value)
		{
            AssertIsLiving();
			var changing = index >= _source.Count || !_source[index].Equals(value);
			if (changing)
				_source[index] = value;
			return changing;
		}

		public int Count
		{
			get
			{
                AssertIsLiving();
				return Sync.Reading(() => _source.Count);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public virtual void Add(T item)
		{
            AssertIsLiving();
			Sync.Modifying(() =>
			{
				_source.Add(item);
				return true;
			});
		}

		public void Add(IEnumerable<T> items)
		{
            AssertIsLiving();
			Sync.Modifying(() =>
			{
				foreach (var item in items)
					Add(item);
			});
		}

		public void Clear()
		{
			Sync.Modifying(
				() => _source.Count != 0,
				() =>
				{
					bool hasItems = Count != 0;
					if (hasItems)
						_source.Clear();
					return hasItems;
				});
		}

		public bool Contains(T item)
		{
            AssertIsLiving();           
			return Sync.Reading(() => _source.Contains(item));
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
            AssertIsLiving();            
			Sync.Reading(() => _source.CopyTo(array, arrayIndex));
		}

		public IEnumerator<T> GetEnumerator()
		{
            AssertIsLiving();
			return _source.GetEnumerator();
		}

		public int IndexOf(T item)
		{
            AssertIsLiving();
			return Sync.Reading(() => _source.IndexOf(item));
		}

		public void Insert(int index, T item)
		{
            AssertIsLiving();
			Sync.Modifying(
			() =>
			{
				_source.Insert(index, item);
				return true;
			});
		}

		public bool Remove(T item)
		{
            AssertIsLiving();
			return Sync.Modifying(
				() => _source.Remove(item));
		}

		public void RemoveAt(int index)
		{
            AssertIsLiving();
			Sync.Modifying(
			() =>
			{
				_source.RemoveAt(index);
				return true;
			});
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
            AssertIsLiving();
			return _source.GetEnumerator();
		}


		public bool Replace(T target, T replacement, bool throwIfNotFound = false)
		{
            AssertIsLiving();
			int index = -1;
			return !target.Equals(replacement) && Sync.Modifying(
				() =>
				{
					index = _source.IndexOf(target);
					if (index == -1)
					{
						if (throwIfNotFound)
							throw new ArgumentException("Not found.", "target");
						return false;
					}
					return true;
				},
				() =>
                    SetValueInternal(index, replacement)
            );
		}

    }
}