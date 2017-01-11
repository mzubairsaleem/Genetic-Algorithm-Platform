/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
			PreviousGenomes = new ConcurrentDictionary<string, TGenome>();
			PreviousGenomesOrder = new ConcurrencyWrapper<string, List<string>>(new List<string>());
		}

		protected readonly ConcurrentDictionary<string, TGenome> PreviousGenomes; // Track by hash...

		protected readonly ConcurrencyWrapper<string, List<string>> PreviousGenomesOrder;

		protected bool Register(TGenome genome)
		{
			var hash = genome.Hash;
			if (PreviousGenomes.TryAdd(hash, genome))
			{
				PreviousGenomesOrder.Add(hash);
				return true;
			}
			return false;
		}

		protected bool Exists(string hash)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");
			return PreviousGenomes.ContainsKey(hash);
		}

		protected bool Exists(TGenome genome)
		{
			if (genome == null)
				throw new ArgumentNullException("genome");
			return Exists(genome.Hash);
		}

		public string[] GetAllPreviousGenomesInOrder()
		{

			return PreviousGenomesOrder.ToArray();
		}

		public abstract TGenome GenerateOne(TGenome[] source = null);

		public IEnumerable<TGenome> Generate(TGenome[] source = null)
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
			return GenerateOne(new TGenome[] { source });
		}

		public IEnumerable<TGenome> Generate(TGenome source)
		{
			var e = new TGenome[] { source };
			while (true) yield return GenerateOne(e);
		}

		protected abstract TGenome MutateInternal(TGenome target);

		public bool AttemptNewMutation(TGenome source, out TGenome mutation, byte triesPerMutationLevel = 5, byte maxMutations = 3)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			return AttemptNewMutation(new TGenome[] { source }, out mutation, triesPerMutationLevel, maxMutations);
		}

		public bool AttemptNewMutation(TGenome[] source, out TGenome genome, byte triesPerMutationLevel = 5, byte maxMutations = 3)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			Debug.Assert(source.Length != 0, "Should never pass an empty source for mutation.");
			if (source.Length != 0)
			{
				// Find one that will mutate well and use it.
				for (byte m = 1; m <= maxMutations; m++)
				{
					for (byte t = 0; t < triesPerMutationLevel; t++)
					{
						genome = Mutate(source.RandomSelectOne(), m);
						if (genome != null && !PreviousGenomes.ContainsKey(genome.Hash))
							return true;
					}
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
				offspring = CrossoverInternal(a, b)?.Where(g => !PreviousGenomes.ContainsKey(g.Hash)).ToArray();
				if (offspring != null && offspring.Length != 0) return true;
				--maxAttempts;
			}

			offspring = null;
			return false;
		}

		// Random matchmaking...  It's possible to include repeats in the source to improve their chances. Possile O(n!) operaion.
		public bool AttemptNewCrossover(TGenome[] source, out TGenome[] offspring, byte maxAttemptsPerCombination = 3)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (source.Length == 2 && source[0] != source[1]) return AttemptNewCrossover((TGenome)source[0], (TGenome)source[1], out offspring, maxAttemptsPerCombination);
			if (source.Length <= 2)
				throw new InvalidOperationException("Must have at least two unique genomes to crossover with.");

			bool isFirst = true;
			do
			{
				// Take one.
				var a = RandomUtilities.RandomSelectOne<TGenome>(source);
				// Get all others (in orignal order/duplicates).
				var s1 = source.Where<TGenome>(g => g != a).ToArray();

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
				source = source.Where<TGenome>(g => g != a).ToArray();
			}
			while (source.Length > 1); // Less than 2 left? Then we have no other options.

			offspring = null;

			return false;
		}

		public bool AttemptNewCrossover(TGenome primary, TGenome[] others, out TGenome[] offspring, byte maxAttemptsPerCombination = 3)
		{
			if (primary == null)
				throw new ArgumentNullException("primary");
			if (others == null)
				throw new ArgumentNullException("source");
			if (others.Length == 0)
				throw new InvalidOperationException("Must have at least two unique genomes to crossover with.");
			if (others.Length == 1 && primary != others[0]) return AttemptNewCrossover(primary, (TGenome)others[0], out offspring, maxAttemptsPerCombination);
			var source = others.Where<TGenome>(g => g != primary);
			if (!source.Any())
				throw new InvalidOperationException("Must have at least two unique genomes to crossover with.");

			// Get all others (in orignal order/duplicates).
			var s1 = source.ToArray();

			// Any left?
			while (s1.Length != 0)
			{
				var b = s1.RandomSelectOne();
				if (AttemptNewCrossover(primary, b, out offspring, maxAttemptsPerCombination)) return true;
				// Reduce the possibilites.
				s1 = s1.Where(g => g != b).ToArray();
				/* ^^^ Why are we filtering like this you might ask? 
				 	   Because the source can have duplicates in order to bias randomness. */
			}

			offspring = null;

			return false;
		}
	}
}



