/*!
 * Origin: http://www.fallingcanbedeadly.com/posts/crazy-extention-methods-tolazylist/
 */

using System;
using System.Collections.Generic;

public class LazyList<T> : IReadOnlyList<T>, IDisposable
{
    public LazyList(IEnumerable<T> source)
    {
        _enumerator = source.GetEnumerator();
        _cached = new List<T>();
    }

    void ThrowIfDisposed()
    {
        if (_cached == null)
            throw new ObjectDisposedException("LazyList");
    }

    public T this[int index]
    {
        get
        {
            ThrowIfDisposed();

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Cannot be less than zero.");
            while (_cached.Count <= index && GetNext()){}
            if (index < _cached.Count)
                return _cached[index];

            throw new ArgumentOutOfRangeException("index", "Great than total count.");
        }
    }

    public int Count
    {
        get
        {
            Finish();
            return _cached.Count;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        ThrowIfDisposed();

        int current = 0;
        while (current < _cached.Count || _enumerator!=null)
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

    public void Dispose()
    {
        var e = _enumerator;
        _enumerator = null;
        if (e != null) e.Dispose();
        var c = _cached;
        if(c!=null) c.Clear();
        _cached = null;
    }

    public int IndexOf(T item)
    {
        ThrowIfDisposed();

        int result = _cached.IndexOf(item);
        T value;
        while (result == -1 && GetNext(out value))
        {
            if(value.Equals(item))
                result = _cached.Count - 1;
        }

        return result;
    }



    public bool Contains(T item)
    {
        return IndexOf(item) != -1;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
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

    List<T> _cached;
    IEnumerator<T> _enumerator;
}