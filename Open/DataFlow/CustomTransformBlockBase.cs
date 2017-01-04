using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Open.DataFlow
{
    public abstract class CustomTransformBlockBase<TInput,TOutput> : IPropagatorBlock<TInput, TOutput>
	{
		protected TransformBlock<TInput, TOutput> Transform;

        protected abstract TransformBlock<TInput, TOutput> InitTransform();

        protected CustomTransformBlockBase()
        {
            Transform = InitTransform();
        }

        public Task Completion
        {
            get
            {
                return Transform.Completion;
            }
        }

        public void Complete()
        {
            Transform.Complete();
        }

        public TOutput ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target, out bool messageConsumed)
        {
            return ((IPropagatorBlock<TInput, TOutput>)Transform).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public void Fault(Exception exception)
        {
            ((IPropagatorBlock<TInput, TOutput>)Transform).Fault(exception);
        }

        public IDisposable LinkTo(ITargetBlock<TOutput> target, DataflowLinkOptions linkOptions)
        {
            return Transform.LinkTo(target, linkOptions);
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source, bool consumeToAccept)
        {
            return ((IPropagatorBlock<TInput, TOutput>)Transform).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            ((IPropagatorBlock<TInput, TOutput>)Transform).ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TOutput> target)
        {
            return ((IPropagatorBlock<TInput, TOutput>)Transform).ReserveMessage(messageHeader, target);
        }
    }
}