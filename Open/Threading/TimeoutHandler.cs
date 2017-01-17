using System;
using System.Threading;
using System.Threading.Tasks;

namespace Open.Threading
{
	public class TimeoutHandler : IDisposable
	{

		CancellationTokenSource TokenSource;
		TimeoutHandler(int delay, Action onComplete)
		{
			TokenSource = new CancellationTokenSource();
			Task.Delay(delay, TokenSource.Token).ContinueWith(t =>
			{
				if (!t.IsCanceled) onComplete();
			});
		}

		public static TimeoutHandler New(int delay, Action onComplete)
		{
			return new TimeoutHandler(delay, onComplete);
		}

		public static bool New(int delay, out IDisposable timeout, Action onComplete)
		{
			timeout = New(delay, onComplete);
			return true;
		}

		public void Dispose()
		{
			TokenSource.Cancel();
		}
	}
}