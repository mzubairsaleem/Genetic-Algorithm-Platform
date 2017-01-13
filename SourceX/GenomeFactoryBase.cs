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
			Registry = new ConcurrentDictionary<string, TGenome>();
			RegistryOrder = new ConcurrencyWrapper<string, List<string>>(new List<string>());
			PreviouslyProduced = new ConcurrentHashSet<string>();
		}

		// Help to reduce copies.
		protected readonly ConcurrentDictionary<string, TGenome> Registry;

		protected readonly ConcurrentHashSet<string> PreviouslyProduced;

		protected readonly ConcurrencyWrapper<string, List<string>> RegistryOrder;

		protected bool Register(TGenome genome, out TGenome actual)
		{
			var hash = genome.Hash;
			if (Registry.TryAdd(hash, genome))
			{
				RegistryOrder.Add(hash);
				actual = genome;
				return true;
			}
			actual = Registry[hash];
			return false;
		}

		protected virtual TGenome Registration(TGenome genome)
		{
			if (genome == null) return null;
			TGenome actual;
			Register(genome, out actual);
			return actual;
		}

		protected bool RegisterProduction(TGenome genome)
		{
			if (genome == null)
				throw new ArgumentNullException("genome");
			var hash = genome.Hash;
			if (!Registry.ContainsKey(hash))
				throw new InvalidOperationException("Registering for production before genome was in global registry.");
			return PreviouslyProduced.Add(genome.Hash);
		}

		protected bool AlreadyProduced(string hash)
		{
			if (hash == null)
				throw new ArgumentNullException("hash");
			return PreviouslyProduced.Contains(hash);
		}

		protected bool AlreadyProduced(TGenome genome)
		{
			if (genome == null)
				throw new ArgumentNullException("genome");
			return AlreadyProduced(genome.Hash);
		}

		public string[] GetAllPreviousGenomesInOrder()
		{

			return RegistryOrder.ToArray();
		}

		// Be sure to call Registration within the GenerateOne call.
		public abstract TGenome GenerateOne(TGenome[] source = null);

		public IEnumerable<TGenome> Generate(TGenome[] source = null)
		{
			while (true)
			{
				var one = GenerateOne(source);
				if (one == null)
				{
					Console.WriteLine("GenomeFactory failed GenerateOne()");
					break;
				}
				yield return one;
			}
		}

		public TGenome GenerateOne(TGenome source)
		{
			var one = GenerateOne(new TGenome[] { source });
			if (one != null)
				RegisterProduction(one);
			return one;
		}

		public IEnumerable<TGenome> Generate(TGenome source)
		{
			var e = new TGenome[] { source };
			while (true) yield return GenerateOne(e);
		}

		// Be sure to call Registration within the GenerateOne call.
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
						if (genome != null && RegisterProduction(genome))
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

		public TGenome[] AttemptNewCrossover(TGenome a, TGenome b, byte maxAttempts = 3)
		{
			while (maxAttempts != 0)
			{
				var offspring = CrossoverInternal(a, b)?.Where(g => RegisterProduction(g)).ToArray();
				if (offspring != null && offspring.Length != 0) return offspring;
				--maxAttempts;
			}

			return null;
		}

		// Random matchmaking...  It's possible to include repeats in the source to improve their chances. Possile O(n!) operaion.
		public TGenome[] AttemptNewCrossover(TGenome[] source, byte maxAttemptsPerCombination = 3)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (source.Length == 2 && source[0] != source[1]) return AttemptNewCrossover((TGenome)source[0], (TGenome)source[1], maxAttemptsPerCombination);
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
					var offspring = AttemptNewCrossover(a, b, maxAttemptsPerCombination);
					if (offspring!=null && offspring.Length!=0) return offspring;
					// Reduce the possibilites.
					s1 = s1.Where(g => g != b).ToArray();
				}

				if (isFirst) // There were no other available candicates to cross over with. :(
					throw new InvalidOperationException("Must have at least two unique genomes to crossover with.");

				// Okay so we've been through all of them with 'a' Now move on to another.
				source = source.Where<TGenome>(g => g != a).ToArray();
			}
			while (source.Length > 1); // Less than 2 left? Then we have no other options.

			return null;
		}

		public TGenome[] AttemptNewCrossover(TGenome primary, TGenome[] others, byte maxAttemptsPerCombination = 3)
		{
			if (primary == null)
				throw new ArgumentNullException("primary");
			if (others == null)
				throw new ArgumentNullException("source");
			if (others.Length == 0)
				throw new InvalidOperationException("Must have at least two unique genomes to crossover with.");
			if (others.Length == 1 && primary != others[0]) return AttemptNewCrossover(primary, (TGenome)others[0], maxAttemptsPerCombination);
			var source = others.Where<TGenome>(g => g != primary);
			if (!source.Any())
				throw new InvalidOperationException("Must have at least two unique genomes to crossover with.");

			// Get all others (in orignal order/duplicates).
			var s1 = source.ToArray();

			// Any left?
			while (s1.Length != 0)
			{
				var b = s1.RandomSelectOne();
				var offspring = AttemptNewCrossover(primary, b, maxAttemptsPerCombination);
				if (offspring!=null && offspring.Length!=0) return offspring;
				// Reduce the possibilites.
				s1 = s1.Where(g => g != b).ToArray();
				/* ^^^ Why are we filtering like this you might ask? 
				 	   Because the source can have duplicates in order to bias randomness. */
			}

			return null;
		}
	}
}



