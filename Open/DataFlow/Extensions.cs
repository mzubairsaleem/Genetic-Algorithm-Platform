using System;
using System.Threading.Tasks;
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

		public static IDisposable LinkToWithExceptions<T>(this ISourceBlock<T> producer, ITargetBlock<T> consumer)
		{
			return producer.LinkTo(consumer, new DataflowLinkOptions() { PropagateCompletion = true });
		}

		public static ISourceBlock<T> Buffer<T>(this ISourceBlock<T[]> source)
		{
			var output = new BufferBlock<T>();
			var input = new ActionBlock<T[]>(array =>
			{
				foreach (var value in array)
					output.Post(value);
			});
			source.LinkToWithExceptions(input);

			return output;
		}

		public static T PropagateFaultsTo<T>(this T source, params IDataflowBlock[] targets)
		where T : IDataflowBlock
		{
			source.Completion.ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					foreach (var target in targets)
					{
						if(target!=null) target.Fault(task.Exception.InnerException);
					}
				};
			});
			return source;
		}

		public static T PropagateCompletionTo<T>(this T source, params IDataflowBlock[] targets)
			where T : IDataflowBlock
		{
			source.Completion.ContinueWith(task =>
			{
				foreach (var target in targets)
				{
					target.Complete();
				}
			});
			return source;
		}
		public static T OnComplete<T>(this T source, Action oncomplete)
			where T : IDataflowBlock
		{
			source.Completion.ContinueWith(task => oncomplete());
			return source;
		}

		public static T OnComplete<T>(this T source, Action<Task> oncomplete)
			where T : IDataflowBlock
		{
			source.Completion.ContinueWith(oncomplete);
			return source;
		}
	}
}