using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Nito.AsyncEx;
using Open.DataFlow;

namespace GeneticAlgorithmPlatform
{
    public static class PoolProcessor
		
	{
		static long BatchId = 0;

		public static IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]> SingleBatch<TGenome>(
			GenomeTestDelegate<TGenome> test,
			int limit = 0) where TGenome : IGenome
		{
			long batchId = Interlocked.Increment(ref BatchId);
			var asyncLock = new AsyncLock();
			var ordered = new SortedDictionary<IFitness, TGenome>();
			var output = new WriteOnceBlock<GenomeFitness<TGenome>[]>(null);

			ITargetBlock<TGenome> reception = new ActionBlock<TGenome>(
				async genome =>
				{
					var fitness = await test(genome, batchId);
					using (await asyncLock.LockAsync())
						ordered.Add(fitness, genome);
				},
				new ExecutionDataflowBlockOptions
				{
					MaxDegreeOfParallelism = 32,
					MaxMessagesPerTask = 3
				});

			if(limit>0)
			{
				reception = reception.AutoCompleteAfter(limit);
			}

			// Eat any repeats.
			reception = reception.Distinct(DataflowMessageStatus.Accepted);

			reception.Completion.ContinueWith(complete =>
			{
				output.Post(ordered.Select(kvp => new GenomeFitness<TGenome>(kvp.Value, kvp.Key)).ToArray());
				output.Complete();
				ordered.Clear();
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
			addLink = task=> {
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