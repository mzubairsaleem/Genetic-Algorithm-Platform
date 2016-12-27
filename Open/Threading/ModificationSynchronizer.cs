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

		bool Modifying(Action action, bool assumeChange = false);

		bool Modifying<T>(ref T target, T newValue);

		// If this is modifiable, it will increment the version.
		void Poke();

	}


	public sealed class ReadOnlyModificationSynchronizer : IModificationSynchronizer
	{

		public void Reading(Action action)
		{
			action();
		}

		public T Reading<T>(Func<T> action)
		{
			return action();
		}

		public bool Modifying(Action action, bool assumeChange = false)
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


	public class ModificationSynchronizer : DisposableBase, IModificationSynchronizer
	{
		public ModificationSynchronizer()
		{
		}

		public event EventHandler Modified;

		protected int _modifyingDepth = 0;
		protected int _version;

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
			Modifying(() => true);
		}


		protected override void OnBeforeDispose()
		{
			Modified = null; // Clean events before swap.
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			Modified = null; // Just in case.
		}


		public virtual void Reading(Action action)
		{
			AssertIsLiving();
			action();
		}

		public virtual T Reading<T>(Func<T> action)
		{
			AssertIsLiving();
			return action();
		}

		protected void SignalModified()
		{
			var handler = Modified;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		public bool Modifying(Func<bool> action)
		{
			return Modifying(null, action);
		}

		public bool Modifying(Action action, bool assumeChange = false)
		{
			return Modifying(() =>
			{
				var ver = _version; // Capture the version so that if changes occur indirectly...
				action();
				return assumeChange || ver != _version;
			});
		}

		public virtual bool Modifying(Func<bool> condition, Func<bool> action)
		{
			AssertIsLiving();
			if (condition != null && !condition())
				return false;

			var ver = _version; // Capture the version so that if changes occur indirectly...
			Interlocked.Increment(ref _modifyingDepth);
			var modified = action();
			if (modified) IncrementVersion();
			// At zero depth and version change? Signal.
			if (Interlocked.Decrement(ref _modifyingDepth) == 0 && ver != _version)
				SignalModified();
			return modified;
		}

		public virtual bool Modifying<T>(ref T target, T newValue)
		{
			AssertIsLiving();
			if (target.Equals(newValue)) return false;

			IncrementVersion();
			target = newValue;

			return true;
		}
	}



	public sealed class ReadWriteModificationSynchronizer : ModificationSynchronizer
	{

		ReaderWriterLockSlim _sync;
		bool _lockOwned;

		public ReadWriteModificationSynchronizer(ReaderWriterLockSlim sync = null)
		{
			if (_sync == null) _lockOwned = true;
			_sync = sync ?? new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		}

		void Cleanup()
		{
			_sync = null;
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			base.OnDispose(calledExplicitly);
			var s = _sync;
			if (!calledExplicitly
			|| !_sync.Write(Cleanup, 10 /* Give any cleanup a chance. */ ))
				Cleanup();
			if (_lockOwned)
			{
				s.Dispose();
			}
		}


		public override void Reading(Action action)
		{
			AssertIsLiving();
			_sync.Read(action);
		}

		public override T Reading<T>(Func<T> action)
		{
			AssertIsLiving();
			return _sync.ReadValue(action);
		}

		public override bool Modifying(Func<bool> condition, Func<bool> action)
		{
			AssertIsLiving();

			// Try and early invalidate.
			if (condition != null && !_sync.ReadValue(condition))
				return false;

			bool modified = false;
			_sync.ReadUpgradeable(() =>
			{
				AssertIsLiving();
				if (condition == null || condition())
				{
					modified = _sync.WriteValue(() => base.Modifying(null, action));
				}
			});
			return modified;
		}


		public override bool Modifying<T>(ref T target, T newValue)
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
					SignalModified();
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