using System;
using System.Threading;

namespace Open.Threading
{

	public interface IReadOnlyModificationSynchronizer
	{
		void Reading(Action action);

		T Reading<T>(Func<T> action);
	}

	public interface IModificationSynchronizer : IReadOnlyModificationSynchronizer
	{

		bool Modifying(Func<bool> condition, Func<bool> action);
		bool Modifying(Func<bool> action);

		bool Modifying(Action action);

		bool Modifying<T>(ref T target, T newValue);

		// If this is modifiable, it will increment the version.
		void Poke();

	}

	public class ReadOnlyModificationSynchronizer : IModificationSynchronizer
	{
		public bool Modifying(Action action)
		{
			throw new NotSupportedException("Synchronizer is read-only.");
		}

		public bool Modifying(Func<bool> action)
		{
			throw new NotSupportedException("Synchronizer is read-only.");
		}

		public bool Modifying(Func<bool> condition, Func<bool> action)
		{
			throw new NotSupportedException("Synchronizer is read-only.");
		}

		public bool Modifying<T>(ref T target, T newValue)
		{
			throw new NotSupportedException("Synchronizer is read-only.");
		}

		public void Reading(Action action)
		{
			action();
		}

		public T Reading<T>(Func<T> action)
		{
			return action();
		}

        public void Poke()
        {
            // Does nothing.
        }

        static ReadOnlyModificationSynchronizer _instance;
		public static ReadOnlyModificationSynchronizer Instance
		{
			get
			{
				return LazyInitializer.EnsureInitialized(ref _instance);
			}
		}
	}

	public sealed class ModificationSynchronizer : DisposableBase, IModificationSynchronizer
	{

		public event EventHandler Modified;

		int _modifyingDepth = 0;
		ReaderWriterLockSlim _sync;
		bool _lockOwned;
		int _version;

		public ModificationSynchronizer(ReaderWriterLockSlim sync = null)
		{
			if (_sync == null) _lockOwned = true;
			_sync = sync ?? new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		}

		void Cleanup()
		{
			Modified = null;
			_sync = null;
		}
		protected override void OnDispose(bool calledExplicitly)
		{
			var s = _sync;
			if (!calledExplicitly
			|| !_sync.Write(Cleanup, 10 /* Give any cleanup a chance. */ ))
				Cleanup();
			if (_lockOwned)
			{
				s.Dispose();
			}

		}

		// public ReaderWriterLockSlim Synchronizer
		// {
		// 	get { return _sync; }
		// }

		public int Version
		{
			get { return _version; }
		}

		public void IncrementVersion()
		{
			Interlocked.Increment(ref _version);
		}

		public void Poke()
		{

		}

		void OnModified()
		{
			var handler = Modified;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}


		public void Reading(Action action)
		{
			AssertIsLiving();
			_sync.Read(action);
		}

		public T Reading<T>(Func<T> action)
		{
			AssertIsLiving();
			return _sync.ReadValue(action);
		}

		public bool Modifying(Func<bool> condition, Func<bool> action)
		{
			AssertIsLiving();

			// Try and early invalidate.
			if(condition!=null && !_sync.ReadValue(condition))
				return false;

			bool modified = false;
			_sync.ReadUpgradeable(() =>
			{
				AssertIsLiving();
				if (condition == null || condition())
				{
					_sync.Write(() =>
					{
						var ver = _version; // Capture the version so that if changes occur indirectly...
						Interlocked.Increment(ref _modifyingDepth);
						modified = action();
						if (modified) IncrementVersion();
						// At zero depth and version change? Signal.
						if(Interlocked.Decrement(ref _modifyingDepth) == 0 && ver != _version)
							OnModified();
					});
				}
			});
			return modified;
		}

		public bool Modifying(Func<bool> action)
		{
			return Modifying(null, action);
		}

		// When using a delegate that doesn't have a boolean return, assume that changes occured.
		public bool Modifying(Action action)
		{
			return Modifying(() =>
			{
				var ver = Version; // Capture the version so that if changes occur indirectly...
				action();
				return ver != Version;
			});
		}

		public bool Modifying<T>(ref T target, T newValue)
		{
			AssertIsLiving();
			if (target.Equals(newValue)) return false;

			bool changed;
			try
			{
				// Note, there's no need for _modifyingDepth recursion tracking here.
				_sync.EnterUpgradeableReadLock();
				AssertIsLiving();

				var ver = _version; // Capture the version so that if changes occur indirectly...
				changed = !target.Equals(newValue);

				try
				{
					_sync.EnterWriteLock();
					if (changed)
					{
						IncrementVersion();
						target = newValue;
					}
				}
				finally
				{
					_sync.ExitWriteLock();
				}

				if (changed)
				{
					// Events will be triggered but this thread will still have the upgradable read.
					OnModified();
				}
			}
			finally
			{
				_sync.ExitUpgradeableReadLock();
			}
			return changed;
		}

    }
}