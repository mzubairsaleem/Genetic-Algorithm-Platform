using System;
using System.Threading.Tasks.Dataflow;
using AlgebraBlackBox;

namespace GeneticAlgorithmPlatform
{

    public class GenomeSelector<TGenome>
	where TGenome : class, IGenome
	{
		internal readonly IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]> Source;
		internal ITargetBlock<TGenome> Promoted;
		internal ITargetBlock<TGenome> Sustained;
		internal ITargetBlock<TGenome> Demoted;

		internal ITargetBlock<TGenome> Origin;

		internal GenomeSelector(
			int poolSize,
			GenomeTestDelegate<TGenome> test)
		{
			if (poolSize < 3)
				throw new ArgumentOutOfRangeException("poolSize", poolSize, "Pool Size must be greater than 2.");

			if (test == null)
				throw new ArgumentNullException();

			Source = PoolProcessor<TGenome>.GenerateTransform(poolSize, test);
			Origin = Source;

			Source.LinkTo(new ActionBlock<GenomeFitness<TGenome>[]>(results =>
			{
				int i = 0, mid = results.Length / 2;
				foreach (var entry in results)
				{
					var genome = entry.Genome;
					// Only the top genome get's promoted.
					if (i == 0)
					{
						Promoted.Post(genome);
						{
							// Console.WriteLine("Promoted: "+entry);
							// Add mutation and variation back into this pool.
							var m = (TGenome)genome.NextMutation();
							if (m != null) Origin.Post(m);
							var v = (TGenome)genome.NextVariation();
							if (v != null) Origin.Post(v);
						}

						var reduced = (genome as Genome).AsReduced() as TGenome;
						if (reduced != genome)
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
					else if (i > mid) { if (Demoted != null) Demoted.Post(genome); }
					else if (Sustained != null) Sustained.Post(genome);
					i++;
				}
			}));
		}

		public static Tuple<ITargetBlock<TGenome>, BroadcastBlock<TGenome>> BuildNetwork(
			int poolSize,
			int depth,
			GenomeTestDelegate<TGenome> test)
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