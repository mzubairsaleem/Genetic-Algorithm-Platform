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
	}
}