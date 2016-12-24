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
    public int SyncVersion
    {
        get { return Sync.Version; }
    }

    protected readonly ModificationSynchronizer Sync;

    public ThreadSafeTrackedList(ModificationSynchronizer sync = null)
    {
        if (sync == null)
        {
            sync = InitNewSync();
        }
        Sync = sync;
        SyncLock = sync.SyncLock;
    }


    protected virtual ModificationSynchronizer InitNewSync()
    {
        return new ModificationSynchronizer(new Object());
    }

    public object SyncLock
    {
        get;
        private set;
    }

    public T this[int index]
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
        return Sync.Modifying(() =>
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

    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    public virtual void Add(T item)
    {
        Sync.Modifying(() =>
        {
            _source.Add(item);
            return true;
        });
    }

    public void Add(IEnumerable<T> items)
    {
        Sync.Modifying(() =>
        {
            foreach (var item in items)
                Add(item);
        });
    }

    public void Clear()
    {
        if (Count != 0)
        {
            Sync.Modifying(() =>
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
        Sync.Modifying(
        () => {
            _source.Insert(index, item);
            return true;
        });
    }

    public bool Remove(T item)
    {
        return Sync.Modifying(
        () => _source.Remove(item));
    }

    public void RemoveAt(int index)
    {
        Sync.Modifying(
        () => {
            _source.RemoveAt(index);
            return true;
        });
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _source.GetEnumerator();
    }


    public bool Replace(T target, T replacement, bool throwIfNotFound = false)
    {
        return !target.Equals(replacement) && Sync.Modifying(() =>
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