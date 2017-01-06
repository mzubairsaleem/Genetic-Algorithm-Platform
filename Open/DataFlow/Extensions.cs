using System.Threading.Tasks.Dataflow;

namespace Open.DataFlow
{
	public static class DataFlowExtensions
	{
		public static ITargetBlock<T> AutoCompleteAfter<T>(this ITargetBlock<T> target, int limit)
		{
			return new AutoCompleteBlock<T>(limit, target);
		}

		public static ITargetBlock<T> Distinct<T>(this ITargetBlock<T> target, DataflowMessageStatus defaultResponseForDuplicate)
		{
			return new DistinctBlock<T>(defaultResponseForDuplicate, target);
		}
	}
}