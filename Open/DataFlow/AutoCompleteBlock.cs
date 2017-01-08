using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open.Threading;

namespace Open.DataFlow
{

	internal class AutoCompleteBlock<T> : ITargetBlock<T>
	{
		private readonly ITargetBlock<T> _target;

		public AutoCompleteBlock(int limit, ITargetBlock<T> target)
		{
			_limit = limit;
			_target = target;
		}

		private readonly int _limit;
		public int Limit
		{
			get { return _limit; }
		}

		private int _allowed = 0;
		public int AllowedCount
		{
			get { return _allowed; }
		}

		public Task Completion
		{
			get
			{
				return _target.Completion;
			}
		}

		public void Complete()
		{
			_target.Complete();
		}

		public void Fault(Exception exception)
		{
			_target.Fault(exception);
		}

		// The key here is to reject the message ahead of time.
		public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
		{
			var result = DataflowMessageStatus.DecliningPermanently;
			var completed = false;
			// There are multiple operations happening here that require synchronization to get right.
			ThreadSafety.LockConditional(_target,
				() => _allowed < _limit,
				() =>
				{
					_allowed++;
					result = _target.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
					completed = _allowed == _limit;
				}
			);

			if (completed) _target.Complete();

			return result;
		}
	}

}