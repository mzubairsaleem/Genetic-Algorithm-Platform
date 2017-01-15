using System.Threading;

namespace GeneticAlgorithmPlatform
{
	public static class SampleID
	{

		static long Current = 0;

		public static long Next()
		{
			return Interlocked.Increment(ref Current);
		}
	}

}
