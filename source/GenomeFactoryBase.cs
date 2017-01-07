/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Open;

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
			yield return Generate();
		}

		public IEnumerable<TGenome> Mutator(TGenome source)
		{
			TGenome next;
			while(AttemptNewMutation(source, out next))
			{
				yield return next;
			}
		}

		public bool AttemptNewMutation(TGenome source, out TGenome mutation, int triesPerMutation = 10)
		{
			return AttemptNewMutation(Enumerable.Repeat(source,1),out mutation, triesPerMutation);
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
					hash = genome==null ? null : genome.Hash;
				}
				while ((hash==null || _previousGenomes.ContainsKey(hash)) && --tries != 0);

				if (tries != 0)
					return true;
			}
			return false;
		}

		public TGenome Generate(TGenome source)
		{
			return Generate(Enumerable.Repeat(source,1));
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
}

