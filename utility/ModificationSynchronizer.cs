using System;

public class ModificationSynchronizer
{

    int _modifyingDepth = 0;
    object _syncLock;
    Action _onModified;


    public ModificationSynchronizer(object syncLock = null, Action onModified = null)
    {
        _syncLock = syncLock ?? new Object();
        _onModified = onModified;
    }
    public ModificationSynchronizer(Action onModified) : this(null, onModified)
    {

    }

    public object SyncLock
    {
        get {
            return _syncLock;
        }
    }

    public bool ReadOnly
    {
        get;
        set;
    }


    public int Version
    {
        get;
        private set;
    }

    public void IncrementVersion()
    {
        lock (_syncLock)
        {
            Version++;
        }
    }

    public bool Modifying(Func<bool> action)
    {
        if (ReadOnly)
            throw new InvalidOperationException("ReadOnly is true.");

        bool modified;
        lock (_syncLock)
        {
            var ver = Version; // Capture the version so that if changes occur indirectly...
            _modifyingDepth++;
            modified = action();
            if(modified) Version++;
            if (--_modifyingDepth == 0 && _onModified != null && ver!=Version) // At zero depth and version change? Signal.
            {
                _onModified();
            }
        }
        return modified;
    }

    // When using a delegate that doesn't have a boolean return, assume that changes occured.
    public bool Modifying(Action action)
    {
        return Modifying(() =>
        {
            var ver = Version; // Capture the version so that if changes occur indirectly...
            action();
            return ver!=Version;
        });
    }

    public bool Modifying<T>(ref T target, T newValue)
    {
        if (ReadOnly)
            throw new InvalidOperationException("ReadOnly is true.");
        
        if(target.Equals(newValue)) return false;

        bool changed;
        lock (_syncLock)
        {
            var ver = Version; // Capture the version so that if changes occur indirectly...
            _modifyingDepth++;
            changed = target.Equals(newValue);
            if(changed) {
                Version++;
                target = newValue;
            }
            if (--_modifyingDepth == 0 && _onModified != null && ver!=Version) // At zero depth and version change? Signal.
            {
                _onModified();
            }
        }
        return changed;
    }
}