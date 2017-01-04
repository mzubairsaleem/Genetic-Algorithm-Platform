using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using AlgebraBlackBox;
using Open.Collections;

namespace GeneticAlgorithmPlatform
{

	public class GenomeSelector<TGenome>
	where TGenome : class, IGenome
	{
		internal readonly IPropagatorBlock<TGenome, TGenome[]> Source;
		internal ITargetBlock<TGenome> Promoted;
		internal ITargetBlock<TGenome> Sustained;
		internal ITargetBlock<TGenome> Demoted;

		internal ITargetBlock<TGenome> Origin;

		internal GenomeSelector(
			int poolSize,
			Func<TGenome, long, Task<IFitness>> test)
		{
			if (poolSize < 3)
				throw new ArgumentOutOfRangeException("poolSize", poolSize, "Pool Size must be greater than 2.");

			if (test == null)
				throw new ArgumentNullException();

			Source = GeneratePool(poolSize, test);
			Origin = Source;

			Source.LinkTo(new ActionBlock<TGenome[]>(results =>
			{
				int i = 0, mid = results.Length / 2;
				foreach (var entry in results)
				{
					// Only the top genome get's promoted.
					if (i == 0)
					{
						Promoted.Post(entry);
						{
							// Console.WriteLine("Promoted: "+entry);
							// Add mutation and variation back into this pool.
							var m = (TGenome)entry.NextMutation();
							if (m != null) Origin.Post(m);
							var v = (TGenome)entry.NextVariation();
							if (v != null) Origin.Post(v);
						}

						var reduced = (entry as Genome).AsReduced() as TGenome;
						if (reduced != entry)
						{
							Promoted.Post(reduced);
							{
								// Console.WriteLine("Promoted: "+entry);
								// Add mutation and variation back into this pool.
								var m = (TGenome)reduced.NextMutation();
								if (m != null) Origin.Post(m);
								var v = (TGenome)reduced.NextVariation();
								if (v != null) Origin.Post(v);
							}
						}
					}
					else if (i > mid) { if (Demoted != null) Demoted.Post(entry); }
					else if (Sustained != null) Sustained.Post(entry);
					i++;
				}
			}));
		}

		static long BatchId = 0;
		static ConcurrentBag<Tuple<HashSet<string>, SortedDictionary<IFitness, TGenome>>>
			BatchPool = new ConcurrentBag<Tuple<HashSet<string>, SortedDictionary<IFitness, TGenome>>>();

		static Tuple<HashSet<string>, SortedDictionary<IFitness, TGenome>> GetBatchTracker()
		{
			Tuple<HashSet<string>, SortedDictionary<IFitness, TGenome>> batch;
			return BatchPool.TryTake(out batch) ? batch : new Tuple<HashSet<string>, SortedDictionary<IFitness, TGenome>>(new HashSet<string>(), new SortedDictionary<IFitness, TGenome>());
		}

		static void ReturnBatchTracker(Tuple<HashSet<string>, SortedDictionary<IFitness, TGenome>> tracker)
		{
			tracker.Item1.Clear();
			tracker.Item2.Clear();
			BatchPool.Add(tracker);
		}

		public static IPropagatorBlock<TGenome, TGenome[]> GeneratePool(
			int poolSize,
			Func<TGenome, long, Task<IFitness>> test)
		{
			// We have to create our own internal buffer and batching to allow for a progressive stream.
			long batchId = Interlocked.Increment(ref BatchId);
			var registry = new Dictionary<long, Tuple<HashSet<string>, SortedDictionary<IFitness, TGenome>>>();

			// Step 2: Process. (attach to a batch ID)
			var testing = new TransformBlock<Tuple<long, TGenome>, Tuple<long, GenomeFitness<TGenome>>>(
				async entry => new Tuple<long, GenomeFitness<TGenome>>(
					entry.Item1, new GenomeFitness<TGenome>(
						entry.Item2, await test(entry.Item2, entry.Item1))),
				new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32 });

			// Step 1: Receieve and filter.
			var reception = new ActionBlock<TGenome>(genome =>
			{
				if (genome != null)
				{
					lock (registry) // Need to synchronize here because the size of the batch matters as well as the batch ID.
					{
						var e = registry.GetOrAdd(batchId, key => GetBatchTracker());
						if (e.Item1.Add(genome.Hash))
						{
							testing.Post(new Tuple<long, TGenome>(batchId, genome));
							if (e.Item1.Count == poolSize)
								batchId = Interlocked.Increment(ref BatchId);
						}
					}
				}
				Debug.WriteLineIf(genome == null, "Cannot process a null Genome.");
			}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

			var output = new BufferBlock<TGenome[]>();

			// Step 3: Buffer (sort) and emit.
			testing.LinkTo(new ActionBlock<Tuple<long, GenomeFitness<TGenome>>>(e =>
			{
				var bId = e.Item1;
				var gf = e.Item2;
				var entry = registry[bId];
				var results = entry.Item2;
				var complete = false;
				lock (results)
				{
					results.Add(gf.Fitness, gf.Genome); // Sorting occurs on adding.
					if (results.Count == poolSize)
					{
						complete = true;
						foreach (var x in results.Where(f => f.Key.Scores.All(s=>s==1)))
							Console.WriteLine("Converged: {0}", x.Value);
						output.Post(results.Values.ToArray());
					}
				}
				if (complete)
				{
					lock (registry)
					{
						registry.Remove(bId);
					}
					ReturnBatchTracker(entry);
				}
			}));

			return DataflowBlock.Encapsulate(reception, output);
		}

		public static Tuple<ITargetBlock<TGenome>, BroadcastBlock<TGenome>> BuildNetwork(
			int poolSize,
			int depth,
			Func<TGenome, long, Task<IFitness>> test)
		{
			if (depth < 2)
				throw new ArgumentOutOfRangeException("depth", depth, "Needs to be greater than 1.");
			/*
				->
				-> ->
				-> -> ->
				-> -> -> ->
				-> -> ->
				-> ->
				->
			*/

			var mid = depth - 1;
			var height = mid * 2 + 1;
			var depths = new GenomeSelector<TGenome>[depth, height];
			var final = new BroadcastBlock<TGenome>(null);

			// First, build the nodes.
			for (var d = 0; d < depth; d++) // 0 = top
			{
				for (var h = d; h < (height - d); h++)
				{
					var gs = new GenomeSelector<TGenome>(poolSize, test);
					depths[d, h] = gs;
				}
			}

			var origin = depths[0, mid].Source;

			// Then with the nodes in place...
			for (var d = 0; d < depth; d++) // 0 = top
			{
				for (var h = d; h < (height - d); h++)
				{
					var gs = depths[d, h];
					gs.Origin = origin;

					var nextDepth = d + 1 < depth ? depths[d + 1, h] : null;
					var nextDemotion = h + 1 < height - d ? depths[0, h + 1] : null;

					if (nextDepth != null) gs.Sustained = nextDepth.Source;
					else if (nextDemotion != null) gs.Sustained = nextDemotion.Source;

					if (nextDemotion != null) gs.Demoted = nextDemotion.Source;

					if (h > 0) gs.Promoted = depths[0, h - 1].Source;
					else gs.Promoted = new ActionBlock<TGenome>(genome =>
					{
						final.Post(genome);
						gs.Source.Post(genome); // Top genome stays in the final pool...
					});

				}
			}

			return new Tuple<ITargetBlock<TGenome>, BroadcastBlock<TGenome>>(depths[0, mid].Source, final);
		}

	}
}