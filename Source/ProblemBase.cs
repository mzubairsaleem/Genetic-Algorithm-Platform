using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
	public abstract class ProblemBase<TGenome> : IProblem<TGenome>
	where TGenome : class, IGenome
	{

		protected readonly ConcurrentDictionary<string, Lazy<GenomeFitness<TGenome, Fitness>>>
			Fitnesses = new ConcurrentDictionary<string, Lazy<GenomeFitness<TGenome, Fitness>>>();

		static int ProblemCount = 0;
		readonly int _id = Interlocked.Increment(ref ProblemCount);
		public int ID { get { return _id; } }

		public ProblemBase()
		{

		}


		// Override this if there is a common key for multiple genomes (aka they are equivalient).
		protected virtual TGenome GetFitnessForKeyTransform(TGenome genome)
		{
			return genome;
		}

		public bool TryGetFitnessFor(TGenome genome, out GenomeFitness<TGenome, Fitness> fitness)
		{
			genome = GetFitnessForKeyTransform(genome);
			var key = genome.Hash;
			Lazy<GenomeFitness<TGenome, Fitness>> value;
			if (Fitnesses.TryGetValue(key, out value))
			{
				fitness = value.Value;
				return true;
			}
			else
			{
				fitness = default(GenomeFitness<TGenome, Fitness>);
				return false;
			}
		}

		public GenomeFitness<TGenome, Fitness>? GetFitnessFor(TGenome genome, bool ensureSourceGenome = false)
		{
			GenomeFitness<TGenome, Fitness> gf;
			TryGetFitnessFor(genome, out gf);
			if (ensureSourceGenome && gf.Genome != genome) // Possible that a 'version' is stored.
				gf = new GenomeFitness<TGenome, Fitness>(genome, gf.Fitness);
			return gf;
		}

		public IEnumerable<IGenomeFitness<TGenome, Fitness>> GetFitnessFor(IEnumerable<TGenome> genomes, bool ensureSourceGenomes = false)
		{
			foreach (var genome in genomes)
			{
				GenomeFitness<TGenome, Fitness> gf;
				if (TryGetFitnessFor(genome, out gf))
				{
					if (ensureSourceGenomes && gf.Genome != genome) // Possible that a 'version' is stored.
						gf = new GenomeFitness<TGenome, Fitness>(genome, gf.Fitness);
					yield return gf;
				}
			}
		}

		public GenomeFitness<TGenome, Fitness> GetOrCreateFitnessFor(TGenome genome)
		{
			if (!genome.IsReadOnly)
				throw new InvalidOperationException("Cannot recall fitness for an unfrozen genome.");
			genome = GetFitnessForKeyTransform(genome);
			var key = genome.Hash;
			return Fitnesses
				.GetOrAdd(key, k =>
					Lazy.New(() => GenomeFitness.New(genome, new Fitness()))).Value;
		}


		public abstract Task<IFitness> ProcessTest(TGenome g, long sampleId = -1);

		GenomeTestDelegate<TGenome> _testProcessor;
		public GenomeTestDelegate<TGenome> TestProcessor
		{
			get
			{
				return LazyInitializer.EnsureInitialized(ref _testProcessor, () => ProcessTest);
			}
		}

		public void AddToGlobalFitness<T>(IEnumerable<T> results) where T : IGenomeFitness<TGenome>
		{
			foreach (var r in results)
				AddToGlobalFitness(r);
		}

		public IFitness AddToGlobalFitness(IGenomeFitness<TGenome> result)
		{
			return AddToGlobalFitness(result.Genome, result.Fitness);
		}

		public IFitness AddToGlobalFitness(TGenome genome, IFitness fitness)
		{
			var global = GetOrCreateFitnessFor(genome).Fitness;
			if (global == fitness)
				throw new InvalidOperationException("Adding fitness on to itself.");
			global.Merge(fitness);
			return global.SnapShot();
		}

		public int GetSampleCountFor(TGenome genome)
		{
			GenomeFitness<TGenome, Fitness> fitness;
			return TryGetFitnessFor(genome, out fitness) ? fitness.Fitness.SampleCount : 0;
		}
	}
}