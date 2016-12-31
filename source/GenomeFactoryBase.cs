/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GeneticAlgorithmPlatform
{

	public abstract class GenomeFactoryBase<TGenome>
	: IGenomeFactory<TGenome>
	where TGenome : class, IGenome
	{

		readonly protected BufferBlock<TGenome>
			Queue = new BufferBlock<TGenome>();


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

		public abstract void Generate(uint count);
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
			if (genome != null)
				Queue.Post(genome);
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


		public IDisposable LinkReception(ITargetBlock<TGenome> block)
		{
			return Queue.LinkTo(block);
		}

		public IDisposable LinkMutation(ISourceBlock<TGenome> block)
		{
			return block.LinkTo(new ActionBlock<TGenome>(genome =>
			{
				Queue.Post(Mutate(genome));
			}));
		}
	}
}

