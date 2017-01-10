/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Open;
using Open.Collections;

namespace GeneticAlgorithmPlatform
{

	public abstract class GenomeFactoryBase<TGenome> : IGenomeFactory<TGenome>
	where TGenome : class, IGenome
	{

		public GenomeFactoryBase()
		{
			_previousGenomes = new ConcurrentDictionary<string, TGenome>();
			_previousGenomesOrder = new ConcurrentList<string>();
		}

		protected readonly ConcurrentDictionary<string, TGenome> _previousGenomes; // Track by hash...
		readonly ConcurrentList<string> _previousGenomesOrder;

		protected bool Register(TGenome genome)
		{
			var hash = genome.Hash;
			if (_previousGenomes.TryAdd(hash, genome))
			{
				_previousGenomesOrder.Add(hash);
				return true;
			}
			return false;
		}

		protected bool Exists(string hash)
		{
			return _previousGenomes.ContainsKey(hash);
		}

		protected bool Exists(TGenome genome)
		{
			return Exists(genome.Hash);
		}

		public string[] PreviousGenomes
		{
			get
			{
				return _previousGenomesOrder.ToArray();
			}
		}

		public abstract TGenome GenerateOne(IEnumerable<TGenome> source = null);

		public IEnumerable<TGenome> Generate(IEnumerable<TGenome> source = null)
		{
			while (true)
			{
				var one = GenerateOne(source);
				if (one == null) break;
				yield return one;
			}
		}

		public TGenome GenerateOne(TGenome source)
		{
			return GenerateOne(Enumerable.Repeat(source, 1));
		}

		public IEnumerable<TGenome> Generate(TGenome source = null)
		{
			var e = Enumerable.Repeat(source, 1);
			while (true) yield return GenerateOne(e);
		}

		protected abstract TGenome MutateInternal(TGenome target);

		public bool AttemptNewMutation(TGenome source, out TGenome mutation, byte triesPerMutationLevel = 5, byte maxMutations = 3)
		{
			return AttemptNewMutation(Enumerable.Repeat(source, 1), out mutation, triesPerMutationLevel, maxMutations);
		}

		public bool AttemptNewMutation(IEnumerable<TGenome> source, out TGenome genome, byte triesPerMutationLevel = 5, byte maxMutations = 3)
		{
			// Find one that will mutate well and use it.
			for (byte m = 1; m <= maxMutations; m++)
			{
				for (byte t = 0; t < triesPerMutationLevel; t++)
				{
					genome = Mutate(source.RandomSelectOne(), m);
					if (genome != null && !_previousGenomes.ContainsKey(genome.Hash))
						return true;
				}
			}
			genome = null;
			return false;
		}


		public IEnumerable<TGenome> Mutate(TGenome source)
		{
			TGenome next;
			while (AttemptNewMutation(source, out next))
			{
				yield return next;
			}
		}

		protected TGenome Mutate(TGenome source, byte mutations = 1)
		{
			TGenome genome = null;
			while (mutations != 0)
			{
				byte tries = 3;
				while (tries != 0 && genome == null)
				{
					genome = MutateInternal(source);
					--tries;
				}
				// Reuse the clone as the source 
				if (genome == null) break; // No single mutation possible? :/
				source = genome;
				--mutations;
			}
			return genome;
		}


		protected abstract TGenome[] CrossoverInternal(TGenome a, TGenome b);

		public bool AttemptNewCrossover(TGenome a, TGenome b, out TGenome[] offspring, byte maxAttempts = 3)
		{
			while (maxAttempts != 0)
			{
				offspring = CrossoverInternal(a, b)?.Where(g => !_previousGenomes.ContainsKey(g.Hash)).ToArray();
				if (offspring != null && offspring.Length != 0) return true;
				--maxAttempts;
			}

			offspring = null;
			return false;
		}

		// Random matchmaking...  It's possible to include repeats in the source to improve their chances. Possile O(n!) operaion.
		public bool AttemptNewCrossover(IEnumerable<TGenome> source, out TGenome[] offspring, byte maxAttemptsPerCombination = 3)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			var s = source as TGenome[] ?? source.ToArray();
			if (s.Length == 2 && s[0] != s[1]) return AttemptNewCrossover(s[0], s[1], out offspring, maxAttemptsPerCombination);
			if (s.Length <= 2)
				throw new InvalidOperationException("Must have at least two unique genomes to crossover with.");

			bool isFirst = true;
			do
			{
				// Take one.
				var a = s.RandomSelectOne();
				// Get all others (in orignal order/duplicates).
				var s1 = s.Where(g => g != a).ToArray();

				// Any left?
				while (s1.Length != 0)
				{
					isFirst = false;
					var b = s1.RandomSelectOne();
					if (AttemptNewCrossover(a, b, out offspring, maxAttemptsPerCombination)) return true;
					// Reduce the possibilites.
					s1 = s1.Where(g => g != b).ToArray();
				}

				if (isFirst) // There were no other available candicates to cross over with. :(
					throw new InvalidOperationException("Must have at least two unique genomes to crossover with.");

				// Okay so we've been through all of them with 'a' Now move on to another.
				s = s.Where(g => g != a).ToArray();
			}
			while (s.Length > 1); // Less than 2 left? Then we have no other options.

			offspring = null;

			return false;
		}
	}


}

