using System;
using System.Collections.Concurrent;
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

		public GenomeFitness<TGenome, Fitness>? GetFitnessFor(TGenome genome)
		{
			GenomeFitness<TGenome, Fitness> gf;
			TryGetFitnessFor(genome, out gf);
			return gf;
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


		protected abstract Task<IFitness> ProcessTest(TGenome g, long sampleId);

		GenomeTestDelegate<TGenome> _testProcessor;
		public GenomeTestDelegate<TGenome> TestProcessor
		{
			get
			{
				return LazyInitializer.EnsureInitialized(ref _testProcessor, () => ProcessTest);
			}
		}

		public IFitness AddToGlobalFitness(IGenomeFitness<TGenome> result)
		{
			return AddToGlobalFitness(result.Genome,result.Fitness);
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
            GenomeFitness<TGenome,Fitness> fitness;
			return TryGetFitnessFor(genome, out fitness) ? fitness.Fitness.SampleCount : 0;
        }
    }
}