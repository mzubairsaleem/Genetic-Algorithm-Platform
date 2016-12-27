/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{

	public abstract class GenomeFactoryBase<TGenome>
	: IGenomeFactory<TGenome>
	where TGenome : IGenome
	{
		public uint MaxGenomeTracking { get; set; }

		protected ConcurrentDictionary<string, TGenome> _previousGenomes; // Track by hash...
		protected ConcurrentQueue<string> _previousGenomesOrder;

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
			return _previousGenomes.TryGetValue(hash, out result) ? result : default(TGenome);
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

		protected abstract IEnumerable<TGenome> GenerateVariations(TGenome source);

		public abstract Task<TGenome> Generate(IEnumerable<TGenome> source);

		public abstract Task<TGenome> MutateAsync(TGenome source, uint mutations);

		public void Add(TGenome genome)
		{
			var hash = genome.Hash;
			if (_previousGenomes.TryAdd(hash, genome))
			{
				_previousGenomesOrder.Enqueue(hash);
			}
		}
	}
}

