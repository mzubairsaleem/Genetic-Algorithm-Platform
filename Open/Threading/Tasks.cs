using System;
using System.Threading.Tasks;

namespace Open.Threading
{
	public static class TaskExtensions
	{
		public static bool IsActive(this Task target)
		{
			if (target == null)
				throw new NullReferenceException();

			switch (target.Status)
			{
				case TaskStatus.Created:
				case TaskStatus.Running:
				case TaskStatus.WaitingForActivation:
				case TaskStatus.WaitingForChildrenToComplete:
				case TaskStatus.WaitingToRun:
					return true;
				case TaskStatus.Canceled:
				case TaskStatus.Faulted:
				case TaskStatus.RanToCompletion:
					return false;
			}

			return false;
		}


		public static Task<T> OnFullfilled<T>(this Task<T> target, Action<T> action)
		{
			target.ContinueWith(task =>
			{
				if (task.IsCompleted) action(task.Result);
			});
			return target;
		}

		public static Task OnFullfilled<T>(this T target, Action action)
			where T : Task
		{
			target.ContinueWith(task =>
			{
				if (task.IsCompleted) action();
			});
			return target;
		}

		// Tasks don't behave like promises so even though this seems like we should call this "Catch", it's not doing that and a real catch statment needs to be wrapped around a wait call.
		public static T OnFaulted<T>(this T target, Action<Exception> action)
			where T : Task
		{
			target.ContinueWith(task =>
			{
				if (task.IsFaulted) action(task.Exception);
			});
			return target;
		}

		public static T OnCancelled<T>(this T target, Action action)
			where T : Task
		{
			target.ContinueWith(task =>
			{
				if (task.IsCanceled) action();
			});
			return target;
		}

	}
}