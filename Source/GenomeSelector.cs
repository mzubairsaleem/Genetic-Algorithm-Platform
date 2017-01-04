using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

		internal GenomeSelector(int poolSize, Func<TGenome, Task<IFitness>> test)
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
						// Add mutation and variation back into this pool.
						var m = (TGenome)entry.NextMutation();
						if(m!=null) Origin.Post(m);
						var v = (TGenome)entry.NextVariation();
						if(v!=null) Origin.Post(v);
					}
					else if (i > mid) { if (Demoted != null) Demoted.Post(entry); }
					else if (Sustained != null) Sustained.Post(entry);
				}
			}));
		}



		public static IPropagatorBlock<TGenome, TGenome[]> GeneratePool(int poolSize, Func<TGenome, Task<IFitness>> test)
		{
			// Step 2: Process.
			var testing = new TransformBlock<TGenome, GenomeFitness<TGenome>>(
				async genome => new GenomeFitness<TGenome>(genome, await test(genome)),
				new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

			// Step 1: Receieve.
			var reception = new ActionBlock<TGenome>(genome =>
			{
				if (genome != null) testing.Post(genome);
				Debug.WriteLineIf(genome == null, "Cannot process a null Genome.");
			});

			var output = new BufferBlock<TGenome[]>();

			// One result/sorted batch at a time.
			var results = new SortedDictionary<IFitness, TGenome>();

			// Step 3: Buffer (sort) and emit.
			testing.LinkTo(new ActionBlock<GenomeFitness<TGenome>>(r =>
			{
				LazyInitializer.EnsureInitialized(ref results);
				results.Add(r.Fitness, r.Genome); // Sorting occurs on adding.
				if (results.Count == poolSize)
				{
					var values = results.Values.ToArray();
					results.Clear(); // Reuse the heap.
					output.Post(values);
				}
			}));

			return DataflowBlock.Encapsulate(testing, output);
		}

		public static Tuple<ITargetBlock<TGenome>, BroadcastBlock<TGenome>> BuildNetwork(
			int poolSize,
			int depth,
			Func<TGenome, Task<IFitness>> test)
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
					var hasNextHeight = h + 1 < height - d;

					if (nextDepth!=null) gs.Sustained = nextDepth.Source;
					else if (hasNextHeight) gs.Sustained = depths[0, h + 1].Source;

					if (hasNextHeight) gs.Demoted = depths[0, h + 1].Source;

					if (h > 0) gs.Promoted = nextDepth!=null ? nextDepth.Source : depths[0, h - 1].Source;
					else gs.Promoted = new ActionBlock<TGenome>(genome =>
					{
						final.Post(genome);
						gs.Source.Post(genome); // Top genome stays in the pool...
					});

				}
			}

			return new Tuple<ITargetBlock<TGenome>, BroadcastBlock<TGenome>>(depths[0, mid].Source, final);
		}

	}
}