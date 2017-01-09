using System;
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

		public static TransformBlock<T, T> Pipe<T>(this ITargetBlock<T> target, Func<T, T> pipe)
		{
			return new TransformBlock<T, T>(pipe);
		}

		public static IDisposable LinkTo<T>(this ISourceBlock<T> producer, Action<T> consumer)
		{
			return producer.LinkTo(new ActionBlock<T>(consumer));
		}

		public static ISourceBlock<T> Buffer<T>(this ISourceBlock<T[]> source)
		{
			var output = new BufferBlock<T>();
			var input = new ActionBlock<T[]>(array =>
			{
				foreach (var value in array)
					output.Post(value);
			});
			source.LinkTo(input);

			return output;
		}
	}
}