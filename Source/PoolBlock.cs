using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GeneticAlgorithmPlatform
{
	public class Poolblock<TGenome> :

		IPropagatorBlock<TGenome, IList<TGenome>>


		where TGenome : IGenome
	{
		readonly TransformManyBlock<TGenome, IList<TGenome>> Transform;

		public readonly int Size;

		public Poolblock(int size)
		{
			if (size <= 0)
				throw new ArgumentOutOfRangeException("size", size, "Must be greater than zero.");

			Size = size;

            // Transform = new TransformManyBlock(genome=>{
            //     yield return null;
            // });
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

		public IList<TGenome> ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IList<TGenome>> target, out bool messageConsumed)
		{
			return ((IPropagatorBlock<TGenome, IList<TGenome>>)Transform).ConsumeMessage(messageHeader, target, out messageConsumed);
		}

		public void Fault(Exception exception)
		{
			((IPropagatorBlock<TGenome, IList<TGenome>>)Transform).Fault(exception);
		}

		public IDisposable LinkTo(ITargetBlock<IList<TGenome>> target, DataflowLinkOptions linkOptions)
		{
			return Transform.LinkTo(target, linkOptions);
		}

		public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TGenome messageValue, ISourceBlock<TGenome> source, bool consumeToAccept)
		{
			return ((IPropagatorBlock<TGenome, IList<TGenome>>)Transform).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
		}

		public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IList<TGenome>> target)
		{
			((IPropagatorBlock<TGenome, IList<TGenome>>)Transform).ReleaseReservation(messageHeader, target);
		}

		public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IList<TGenome>> target)
		{
			return ((IPropagatorBlock<TGenome, IList<TGenome>>)Transform).ReserveMessage(messageHeader, target);
		}
	}
}