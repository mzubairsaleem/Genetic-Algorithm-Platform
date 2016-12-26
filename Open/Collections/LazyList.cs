/*!
 * @author electricessence / https://github.com/electricessence/
 * Origin: http://www.fallingcanbedeadly.com/posts/crazy-extention-methods-tolazylist/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;

namespace Open.Collections
{
	public class LazyList<T> : DisposableBase, IReadOnlyList<T>
	{       
        List<T> _cached;
		IEnumerator<T> _enumerator;
		public LazyList(IEnumerable<T> source)
		{
			_enumerator = source.GetEnumerator();
			_cached = new List<T>();
		}

        protected override void OnDispose(bool calledExplicitly)
        {
            var e = _enumerator;
			_enumerator = null;
			e.SmartDispose();
			var c = _cached;
			_cached = null;
			c.SmartDispose();
        }

		public T this[int index]
		{
			get
			{
				AssertIsLiving();

				if (index < 0)
					throw new ArgumentOutOfRangeException("index", "Cannot be less than zero.");
				while (_cached.Count <= index && GetNext()) { }
				if (index < _cached.Count)
					return _cached[index];

				throw new ArgumentOutOfRangeException("index", "Great than total count.");
			}
		}

		public int Count
		{
			get
			{
                AssertIsLiving();
				Finish();
				return _cached.Count;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			AssertIsLiving();

			int current = 0;
			while (current < _cached.Count || _enumerator != null)
			{
				if (current == _cached.Count)
				{
					T value;
					if (GetNext(out value))
					{
						yield return value;
					}
					else
					{
						yield break;
					}
				}
				else
				{
					yield return _cached[current];
				}

				current++;
			}
		}

		public int IndexOf(T item)
		{
			AssertIsLiving();

			int result = _cached.IndexOf(item);
			T value;
			while (result == -1 && GetNext(out value))
			{
				if (value.Equals(item))
					result = _cached.Count - 1;
			}

			return result;
		}



		public bool Contains(T item)
		{
            AssertIsLiving();
			return IndexOf(item) != -1;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
            AssertIsLiving();
			foreach (var item in this)
				array[arrayIndex++] = item;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private bool GetNext(out T value)
		{
			if (_enumerator != null)
			{
				if (_enumerator.MoveNext())
				{
					value = _enumerator.Current;
					_cached.Add(value);
					return true;
				}
				else
				{
					var e = _enumerator;
					_enumerator = null;
					if (e != null) e.Dispose();
				}
			}
			value = default(T);
			return false;
		}

		private bool GetNext()
		{
			T value;
			return GetNext(out value);
		}

		private void Finish()
		{
			while (_enumerator != null)
				GetNext();
		}

	}
}