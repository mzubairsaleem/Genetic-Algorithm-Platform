using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Open.DataFlow
{

    internal class DistinctBlock<T> : ITargetBlock<T>
	{
		private readonly ITargetBlock<T> _target;
		private readonly DataflowMessageStatus _defaultResponseForDuplicate;

		private readonly HashSet<T> _set = new HashSet<T>();

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
			bool didntHave;
			lock (_target) // Assure order of acceptance.
				didntHave = _set.Add(messageValue);
			if (didntHave)
				return _target.OfferMessage(messageHeader, messageValue, source, consumeToAccept);

			return _defaultResponseForDuplicate;
		}
	}

}