using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Open.DataFlow;

namespace GeneticAlgorithmPlatform
{
	public class PoolProcessor<TGenome>
		where TGenome : IGenome

	{
		public readonly int Size;

		readonly ConcurrentQueue<TGenome> Queue = new ConcurrentQueue<TGenome>();
		readonly IProblem<TGenome> Problem;

		IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]> _transform;
		public PoolProcessor(
			IProblem<TGenome> problem,
			int size)
		{
			Size = size;
			Problem = problem;
			Selected = new BroadcastBlock<TGenome[]>(g => g.ToArray());
			Next();
		}

		public readonly BroadcastBlock<TGenome[]> Selected;


		public void Next()
		{
			var old = Interlocked.Exchange(ref _transform, Processor(Problem.TestProcessor, Size));
			if (old != null) old.Complete(); // Make sure to release this.
			_transform.Completion.ContinueWith(task =>
			{
				var results = _transform.Receive();
				// We must document fitness results even if we don't select them.
				foreach (var entry in results)
					Problem.AddToGlobalFitness(entry);

				Selected.Post(results.Select(r => r.Genome).Take(results.Length / 2 + 1).ToArray());
			});

			TGenome peek;
			while (Queue.TryPeek(out peek) && Post(peek))
			{
				TGenome dequeued;
				var removed = Queue.TryDequeue(out dequeued);
				Debug.Assert(removed && dequeued.Equals(peek), "Should have occured and been equal.");
			}
		}

		public int Queued
		{
			get { return Queue.Count; }
		}

		public bool Post(TGenome genome)
		{
			if (_transform.Post(genome))
			{
				// Console.WriteLine("Posting.");
				return true;
			}
			else
			{
				// Console.WriteLine("Queueing.");
				Queue.Enqueue(genome);
				return false;
			}
		}

		static long BatchId = 0;

		static IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]> Processor(
			GenomeTestDelegate<TGenome> test,
			int size)
		{
			if (size < 1)
				throw new ArgumentOutOfRangeException("size", size, "Must be at least 1.");

			long batchId = Interlocked.Increment(ref BatchId);
			var output = new WriteOnceBlock<GenomeFitness<TGenome>[]>(null);
			var results = new GenomeFitness<TGenome>[size];
			int index = -1;

			ITargetBlock<TGenome> reception = new ActionBlock<TGenome>(
				async genome => results[Interlocked.Increment(ref index)]
					= new GenomeFitness<TGenome>(genome, await test(genome, batchId)),
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


	}

}