using System.Collections.Generic;

namespace GeneticAlgorithmPlatform
{
	// Use in place of a tuple.
	// Struct guidelines: https://msdn.microsoft.com/en-us/library/ms229017(v=vs.110).aspx
	// This is basically a KeyValuePair but more custom.

	public interface IGenomeFitness<TGenome>
		where TGenome : IGenome
	{
		TGenome Genome { get; }
		IFitness Fitness { get; }
	}
	public interface IGenomeFitness<TGenome, TFitness> : IGenomeFitness<TGenome>
		where TGenome : IGenome
		where TFitness : IFitness
	{

		new TFitness Fitness { get; }
	}



	public struct GenomeFitness<TGenome, TFitness> : IGenomeFitness<TGenome, TFitness>
		where TGenome : IGenome
		where TFitness : IFitness
	{
		public TGenome Genome { get; private set; }
		public TFitness Fitness { get; private set; }

		IFitness IGenomeFitness<TGenome>.Fitness
		{
			get
			{
				return this.Fitness;
			}
		}

		public GenomeFitness(TGenome genome, TFitness Fitness)
		{
			this.Genome = genome;
			this.Fitness = Fitness;
		}
	}

	public struct GenomeFitness<TGenome> : IGenomeFitness<TGenome>
		where TGenome : IGenome
	{
		public TGenome Genome { get; private set; }
		public IFitness Fitness { get; private set; }

		public GenomeFitness(TGenome genome, IFitness Fitness)
		{
			this.Genome = genome;
			this.Fitness = Fitness;
		}
	}

	public static class GenomeFitness
	{

		public static GenomeFitness<TGenome> SnapShot<TGenome>(this IGenomeFitness<TGenome> source)
			where TGenome : IGenome
		{
			return new GenomeFitness<TGenome>(source.Genome, source.Fitness.SnapShot());
		}
		public static GenomeFitness<TGenome, TFitness> New<TGenome, TFitness>(TGenome genome, TFitness fitness)
			where TGenome : IGenome
			where TFitness : IFitness
		{
			return new GenomeFitness<TGenome, TFitness>(genome, fitness);
		}

		internal static IGenomeFitness<TGenome, TFitness> GFFromGF<TGenome, TFitness>(this KeyValuePair<TGenome, TFitness> kvp)
			where TGenome : IGenome
			where TFitness : IFitness
		{
			return new GenomeFitness<TGenome, TFitness>(kvp.Key, kvp.Value);
		}

		internal static IGenomeFitness<TGenome, TFitness> GFFromFG<TGenome, TFitness>(this KeyValuePair<TFitness, TGenome> kvp)
			where TGenome : IGenome
			where TFitness : IFitness
		{
			return new GenomeFitness<TGenome, TFitness>(kvp.Value, kvp.Key);
		}


	}
}