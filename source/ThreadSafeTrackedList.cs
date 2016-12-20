/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections;
using System.Collections.Generic;


public class ThreadSafeTrackedList<T> : IList<T>
{
    readonly List<T> _source = new List<T>();

    virtual public T this[int index]
    {
        get
        {
            return _source[index];
        }

        set
        {
            SetValue(index, value);
        }
    }

    public bool SetValue(int index, T value)
    {
        return Modifying(() =>
         {
             var changing = index > _source.Count || !_source[index].Equals(value);
             if (changing)
                 _source[index] = value;
             return changing;
         });
    }

    public int Count
    {
        get
        {
            return _source.Count;
        }
    }

    virtual protected void OnModified()
    {

    }

    int _modifyingDepth;
    protected bool Modifying(Func<bool> action)
    {
        bool modified;
        lock (_source)
        {
            _modifyingDepth++;
            modified = action();
            if (--_modifyingDepth == 0 && modified)
                OnModified();
        }
        return modified;
    }

    protected void Modifying(Action action)
    {
        if (IsReadOnly)
            throw new InvalidOperationException("This collection is set to read-only.");

        lock (_source)
        {
            _modifyingDepth++;
            action();
            if (--_modifyingDepth == 0)
                OnModified();
        }
    }

    virtual public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    public void Add(T item)
    {
        Modifying(() => _source.Add(item));
    }

    public void Add(IEnumerable<T> items)
    {
        Modifying(() =>
        {
            foreach (var item in items)
                Add(item);
        });
    }

    public void Clear()
    {
        if (Count != 0)
        {
            Modifying(() =>
            {
                bool hasItems = Count != 0;
                if (hasItems)
                    _source.Clear();
                return hasItems;
            });
        }

    }

    public bool Contains(T item)
    {
        return _source.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _source.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _source.GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return _source.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        Modifying(() => _source.Insert(index, item));
    }

    public bool Remove(T item)
    {
        return Modifying(() => _source.Remove(item));
    }

    public void RemoveAt(int index)
    {
        Modifying(() => _source.RemoveAt(index));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _source.GetEnumerator();
    }


    public bool Replace(T target, T replacement, bool throwIfNotFound = false)
    {
        return Modifying(() =>
        {
            var index = _source.IndexOf(target);
            if (index == -1)
            {
                if (throwIfNotFound)
                    throw new ArgumentException("target", "gene not found.");
                return false;
            }

            return SetValue(index, replacement);
        });
    }
}