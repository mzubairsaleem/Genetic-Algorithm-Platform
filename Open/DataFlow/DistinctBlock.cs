using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Open.DataFlow
{

	internal class DistinctBlock<T> : ITargetBlock<T>
	{
		private readonly ITargetBlock<T> _target;
		private readonly DataflowMessageStatus _defaultResponseForDuplicate;

		private readonly ConcurrentDictionary<T, bool> _set = new ConcurrentDictionary<T, bool>();

		public DistinctBlock(DataflowMessageStatus defaultResponseForDuplicate, ITargetBlock<T> target)
		{
			_target = target;
			_defaultResponseForDuplicate = defaultResponseForDuplicate;
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
			return _set.TryAdd(messageValue, true)
				? _target.OfferMessage(messageHeader, messageValue, source, consumeToAccept)
				: _defaultResponseForDuplicate;
		}
	}

}