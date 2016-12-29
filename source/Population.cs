/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
	public class Population<TGenome> : ConcurrentDictionary<string, TGenome>
	where TGenome : IGenome

	{
		private IGenomeFactory<TGenome> _genomeFactory;

		public Population(IGenomeFactory<TGenome> genomeFactory)
		{
			_genomeFactory = genomeFactory;
		}


		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}


		public void Add(TGenome potential)
		{
			string ts;
			if (potential != null && !ContainsKey(ts = potential.Hash))
			{
				if (!potential.IsReadOnly)
					throw new InvalidOperationException("Adding a genome that is not frozen is prohibited.");

				TryAdd(ts, potential);
			}
		}
		public async Task Add()
		{
			// Be sure to add randomness in...
			Add(await _genomeFactory.GenerateAsync().ConfigureAwait(false));
		}

		public void Add(IEnumerable<TGenome> genomes)
		{
			if (genomes != null)
				foreach (var g in genomes)
					Add(g);
		}

		void AddVariation(IEnumerable<TGenome> rankedGenomes = null)
		{
			if (rankedGenomes != null)
			{
				// Ensure only 1 enumerator created to peek into collection.
				using (var e = rankedGenomes.GetEnumerator())
				{
					if (e.MoveNext())
					{
						var top = e.Current;
						var v = top.NextVariation();
						if (v != null)
							Add((TGenome)v);
					}
				}
			}
		}

		public void Populate(int count, IEnumerable<TGenome> rankedGenomes = null)
		{
			AddVariation(rankedGenomes);

			// Then add mutations from best in source.
			for (var i = 0; i < count; i++)
			{
				Add(_genomeFactory.Generate(rankedGenomes));
			}

		}

		public async Task PopulateAsync(int count, IEnumerable<TGenome> rankedGenomes = null)
		{
			AddVariation(rankedGenomes);

			// Then add mutations from best in source.
			for (var i = 0; i < count; i++)
			{
				Add(await _genomeFactory.GenerateAsync(rankedGenomes));
			}

		}

		// Provide a mechanism for culling the herd without requiring IProblem to be imported.
		public void KeepOnly(IEnumerable<TGenome> selected)
		{
			var hashed = new HashSet<string>(selected.Select(o => o.Hash));
			foreach (var o in this.ToArray()) // Make a copy first...
			{
				TGenome g;
				var key = o.Key;
				if (!hashed.Contains(key))
					TryRemove(key, out g);
			}
		}

	}
}