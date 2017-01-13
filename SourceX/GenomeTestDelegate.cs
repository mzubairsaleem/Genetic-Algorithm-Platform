using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
	public delegate Task<IFitness> GenomeTestDelegate<TGenome>(TGenome candidate, long sampleId) where TGenome : IGenome;
}