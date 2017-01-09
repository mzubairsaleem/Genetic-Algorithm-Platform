/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open;
using Open.Collections;

namespace GeneticAlgorithmPlatform
{

	public abstract class GenomeFactoryBase<TGenome> : IGenomeFactory<TGenome>
	where TGenome : class, IGenome
	{

		public uint MaxGenomeTracking { get; set; }

		protected ConcurrentDictionary<string, TGenome> _previousGenomes; // Track by hash...
		ConcurrentQueue<string> _previousGenomesOrder;

		public GenomeFactoryBase()
		{
			MaxGenomeTracking = 10000;
			_previousGenomes = new ConcurrentDictionary<string, TGenome>();
			_previousGenomesOrder = new ConcurrentQueue<string>();
		}

		public string[] PreviousGenomes
		{
			get
			{
				return _previousGenomesOrder.ToArray();
			}
		}

		public TGenome GetPrevious(string hash)
		{
			TGenome result;
			return _previousGenomes.TryGetValue(hash, out result) ? result : null;
		}

		Task _trimmer;
		public Task TrimPreviousGenomes()
		{
			var _ = this;
			var t = _trimmer;
			if (t != null)
				return t;

			return LazyInitializer.EnsureInitialized(ref _trimmer,
			() => Task.Run(() =>
			{
				while (_previousGenomesOrder.Count > MaxGenomeTracking)
				{
					string next;
					if (_previousGenomesOrder.TryDequeue(out next))
					{
						TGenome g;
						this._previousGenomes.TryRemove(next, out g);
					}
				}

				Interlocked.Exchange(ref _trimmer, null);
			}));
		}



		public IEnumerable<TGenome> Generator()
		{
			while (true) yield return Generate();
		}

		public IEnumerable<TGenome> Mutator(TGenome source)
		{
			TGenome next;
			while (AttemptNewMutation(source, out next))
			{
				yield return next;
			}
		}

		public bool AttemptNewMutation(TGenome source, out TGenome mutation, int triesPerMutation = 10)
		{
			return AttemptNewMutation(Enumerable.Repeat(source, 1), out mutation, triesPerMutation);
		}

		public bool AttemptNewMutation(IEnumerable<TGenome> source, out TGenome genome, int triesPerMutation = 10)
		{
			genome = null;
			string hash = null;
			// Find one that will mutate well and use it.
			for (uint m = 1; m < 4; m++) // Mutation count
			{
				var tries = triesPerMutation;//200;
				do
				{
					genome = Mutate(source.RandomSelectOne(), m);
					hash = genome == null ? null : genome.Hash;
				}
				while ((hash == null || _previousGenomes.ContainsKey(hash)) && --tries != 0);

				if (tries != 0)
					return true;
			}
			return false;
		}

		public TGenome Generate(TGenome source)
		{
			return Generate(Enumerable.Repeat(source, 1));
		}
		public abstract TGenome Generate(IEnumerable<TGenome> source = null);
		protected abstract TGenome MutateInternal(TGenome target);

		protected TGenome Mutate(TGenome source, uint mutations = 1)
		{
			TGenome genome = null;
			for (uint i = 0; i < mutations; i++)
			{
				uint tries = 3;
				do
				{
					genome = MutateInternal(source);
				}
				while (genome == null && --tries != 0);
				// Reuse the clone as the source 
				if (genome == null) break; // No mutation possible? :/
				source = genome;
			}
			return genome;
		}

		protected void Register(TGenome genome)
		{
			var hash = genome.Hash;
			if (_previousGenomes.TryAdd(hash, genome))
			{
				_previousGenomesOrder.Enqueue(hash);
			}
		}


	}


	public class GenomeProducer<TGenome> : ISourceBlock<TGenome>
		where TGenome : IGenome
	{

		BufferBlock<TGenome> EnqueuedBuffer = new BufferBlock<TGenome>();
		BufferBlock<TGenome> ProduceBuffer = new BufferBlock<TGenome>(new DataflowBlockOptions { BoundedCapacity = 100 });
		BufferBlock<TGenome> OutputBuffer;

		ActionBlock<bool> Producer;
		HashSet<string> Registry = new HashSet<string>();

		private GenomeProducer(IEnumerator<TGenome> source, int bufferSize = 100)
		{
			OutputBuffer = new BufferBlock<TGenome>(new DataflowBlockOptions
			{
				BoundedCapacity = bufferSize
			});
			// Should make the enqueue buffer the priority.
			EnqueuedBuffer.LinkTo(OutputBuffer);
			ProduceBuffer.LinkTo(OutputBuffer);

			Producer = new ActionBlock<bool>(async retry =>
			{
				int attempts = 0;
				bool more = false;
				while (attempts++ < 20 && (more = source.MoveNext()))
				{
					var next = source.Current;
					if (next != null)
					{
						var hash = next.Hash;
						if (Registry.Contains(hash)) continue;
						// Try and reserve this hash.
						lock (Registry)
						{
							// Don't own it? :(
							if (!Registry.Add(hash)) continue;
						}
						Debug.WriteLine("Produce Buffer: " + ProduceBuffer.Count);
						// Producer magic happens here...
						if (await ProduceBuffer.SendAsync(next))
						{
							attempts = 0;
						}
						else
						{
							lock (Registry) Registry.Remove(hash);
							// This does not mean postponed.  Should have not rejected unless complete.
							more = false;
							break;
						}
					}
				}

				if (!more)
				{
					Producer.Complete();
					ProduceBuffer.Complete();
				}
				else
				{
					Producer.Post(true);
				}
			});

			Producer.Post(true);
		}

		public GenomeProducer(
			IEnumerable<TGenome> source,
			int bufferSize = 100) : this(source.PreCache(2).GetEnumerator(), bufferSize)
		{

		}

		bool TryEnqueueInternal(BufferBlock<TGenome> target, TGenome genome)
		{
			bool queued = false;
			if (genome != null)
			{
				var hash = genome.Hash;
				if (!Registry.Contains(hash))
				{
					lock (Registry)
					{
						if (!Registry.Contains(hash) && target.Post(genome))
						{
							queued = Registry.Add(genome.Hash);
							Debug.Assert(queued);
						}
					}
				}
			}
			return queued;
		}

		public bool TryEnqueue(TGenome genome)
		{
			return TryEnqueueInternal(EnqueuedBuffer, genome);
		}

		// bool TryEnqueueProduction(TGenome genome)
		// {
		// 	return TryEnqueueInternal(ProduceBuffer, genome);
		// }

		public Task Completion
		{
			get
			{
				return OutputBuffer.Completion;
			}
		}

		public void Complete()
		{
			Producer.Complete();
			ProduceBuffer.Complete();
			EnqueuedBuffer.Complete();
			OutputBuffer.Complete();
		}

		public TGenome ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TGenome> target, out bool messageConsumed)
		{
			return ((ISourceBlock<TGenome>)OutputBuffer).ConsumeMessage(messageHeader, target, out messageConsumed);
		}

		public void Fault(Exception exception)
		{
			((ISourceBlock<TGenome>)OutputBuffer).Fault(exception);
		}

		public IDisposable LinkTo(ITargetBlock<TGenome> target, DataflowLinkOptions linkOptions)
		{
			return OutputBuffer.LinkTo(target, linkOptions);
		}

		public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TGenome> target)
		{
			((ISourceBlock<TGenome>)OutputBuffer).ReleaseReservation(messageHeader, target);
		}

		public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TGenome> target)
		{
			return ((ISourceBlock<TGenome>)OutputBuffer).ReserveMessage(messageHeader, target);
		}

	}

}

