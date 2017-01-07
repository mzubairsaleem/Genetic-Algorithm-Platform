using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open.DataFlow;

namespace GeneticAlgorithmPlatform
{
    public static class PoolProcessor

	{
		static long BatchId = 0;

		public static IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]> SingleBatch<TGenome>(
			GenomeTestDelegate<TGenome> test,
			int size) where TGenome : IGenome
		{
			if (size < 1)
				throw new ArgumentOutOfRangeException("size", size, "Must be at least 1.");

			long batchId = Interlocked.Increment(ref BatchId);
			var output = new WriteOnceBlock<GenomeFitness<TGenome>[]>(null);
			var results = new GenomeFitness<TGenome>[size];
			int index = -1;

			ITargetBlock<TGenome> reception = new ActionBlock<TGenome>(
				async genome => results[Interlocked.Increment(ref index)] = new GenomeFitness<TGenome>(genome, await test(genome, batchId)),
				new ExecutionDataflowBlockOptions
				{
					MaxDegreeOfParallelism = 32,
					MaxMessagesPerTask = 3
				})
				.AutoCompleteAfter(size)
				// Eat any repeats.
				.Distinct(DataflowMessageStatus.Accepted);

			reception.Completion.ContinueWith(complete =>
			{
				Array.Sort(results, GenomeFitness.Comparison);
				output.Post(results);
				output.Complete();
			});

			return DataflowBlock.Encapsulate(reception, output);
		}


		public static IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]> GenerateTransform<TGenome>(
			int poolSize,
			GenomeTestDelegate<TGenome> test) where TGenome : IGenome
		{
			var input = new BufferBlock<TGenome>();
			var output = new BufferBlock<GenomeFitness<TGenome>[]>();

			Action<Task> addLink = null;
			addLink = task =>
			{
				var batch = SingleBatch(test, poolSize);
				batch.Completion.ContinueWith(addLink);
				batch.LinkTo(output);
				input.LinkTo(batch);
			};

			// Prelink these...
			addLink(null);
			addLink(null);
			addLink(null);

			return DataflowBlock.Encapsulate(input, output);
		}

	}

}