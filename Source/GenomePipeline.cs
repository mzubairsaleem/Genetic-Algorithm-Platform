using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Open.Collections;

namespace GeneticAlgorithmPlatform
{
	// GenomeSelection should be short lived.
	public struct GenomeSelection<TGenome>
	{
		public readonly TGenome[] Rejected;
		public readonly TGenome[] Selected;

		public GenomeSelection(TGenome[] selected, TGenome[] rejected)
		{
			Selected = selected;
			Rejected = rejected;
		}
	}

	public static class GenomePipeline
	{
		static long BatchID = 0;

		public static long UniqueBatchID()
		{
			return Interlocked.Increment(ref BatchID);
		}

		public static IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]>
			Processor<TGenome>(
				GenomeTestDelegate<TGenome> test,
				int size)
			where TGenome : IGenome
		{
			var input = new BatchBlock<TGenome>(size);
			var output = new TransformBlock<TGenome[], GenomeFitness<TGenome>[]>(async batch =>
			{
				long batchId = UniqueBatchID();
				return await Task.WhenAll(batch.Select(async g => new GenomeFitness<TGenome>(g, await test(g, batchId))))
					.ContinueWith(task => task.Result.OrderBy(g => g, GenomeFitness.Comparer<TGenome>.Instance)
						.ToArray());
			});

			input.LinkTo(output);

			return DataflowBlock.Encapsulate(input, output);
		}

		public static TransformBlock<GenomeFitness<TGenome>[], GenomeSelection<TGenome>>
			Selector<TGenome>(
				IProblem<TGenome> problem)
			where TGenome : IGenome
		{
			if (problem == null)
				throw new ArgumentNullException("problem");

			return new TransformBlock<GenomeFitness<TGenome>[], GenomeSelection<TGenome>>(results =>
			{
				var len = results.Length;
				var selectionPoint = len / 2;
				var selected = new TGenome[selectionPoint];
				var rejected = new TGenome[len - selectionPoint];
				for (var i = 0; i < len; i++)
				{
					var gf = results[i];
					problem.AddToGlobalFitness(gf);
					if (i < selectionPoint)
						selected[i] = gf.Genome;
					else
						rejected[i - selectionPoint] = gf.Genome;
				}
				return new GenomeSelection<TGenome>(selected, rejected);
			});
		}

		public static IPropagatorBlock<TGenome, GenomeSelection<TGenome>>
			Selector<TGenome>(
				IProblem<TGenome> problem,
					int size)
			where TGenome : IGenome
		{
			if (problem == null)
				throw new ArgumentNullException("problem");
			if (size < 2)
				throw new ArgumentOutOfRangeException("size", size, "Must be at least 2.");				

			var processor = Processor(problem.TestProcessor, size);
			var selector = Selector(problem);
			processor.LinkTo(selector);

			return DataflowBlock.Encapsulate(processor, selector);
		}

		public static ITargetBlock<TGenome>
			Distributor<TGenome>(
				IProblem<TGenome> problem,
				int size,
				ITargetBlock<TGenome[]> selected,
				ITargetBlock<TGenome[]> rejected = null)
			where TGenome : IGenome
		{
			if (selected == null)
				throw new ArgumentNullException("selected");

			var input = Selector(problem, size);
			input.LinkTo(new ActionBlock<GenomeSelection<TGenome>>(selection =>
			{
				selected.Post(selection.Selected);
				if (rejected != null)
					rejected.Post(selection.Rejected);
			}));
			return input;
		}

		public static ITargetBlock<TGenome>
			Distributor<TGenome>(
				IProblem<TGenome> problem,
				int size,
				Action<TGenome[]> selected,
				Action<TGenome[]> rejected = null)
			where TGenome : IGenome
		{
			return Distributor(problem, size,
				new ActionBlock<TGenome[]>(selected),
				rejected == null ? null : new ActionBlock<TGenome[]>(rejected));
		}

		public static ISourceBlock<TGenome> Node<TGenome>(
			IEnumerable<ISourceBlock<TGenome>> sources,
			IProblem<TGenome> problem,
			int size,
			Action<TGenome[]> globalHandler = null)
			where TGenome : IGenome
		{
			var output = new BufferBlock<TGenome>();
			var processor = Distributor(problem, size, selected =>
			{
				foreach (var g in selected)
					output.Post(g);

				if(globalHandler!=null)
					globalHandler(selected);
			});
			foreach (var source in sources)
			{
				source.LinkTo(processor);
			}

			return output;
		}

		public static ISourceBlock<TGenome> Node<TGenome>(
			ISourceBlock<TGenome> source,
			IProblem<TGenome> problem,
			int size,
			Action<TGenome[]> globalHandler = null)
			where TGenome : IGenome
		{
			return Node(new ISourceBlock<TGenome>[] { source }, problem, size, globalHandler);
		}


	}

	public class GenomePipelineBuilder<TGenome>
		where TGenome : IGenome
	{
		public readonly ushort PoolSize;
		public readonly byte SourceCount;

		readonly GenomeProducer<TGenome> Producer;

		readonly IProblem<TGenome> Problem;

		readonly Action<TGenome[]> GlobalSelectedHandler;

		public GenomePipelineBuilder(
			GenomeProducer<TGenome> producer,
			IProblem<TGenome> problem,
			ushort poolSize,
			byte sourceCount = 2,
			Action<TGenome[]> globalSelectedHandler = null)
		{
			if (sourceCount < 1)
				throw new ArgumentOutOfRangeException("sourceCount", sourceCount, "Must be at least 1.");
			if (producer == null)
				throw new ArgumentNullException("producer");
			if (problem == null)
				throw new ArgumentNullException("problem");
			Producer = producer;
			Problem = problem;
			PoolSize = poolSize;
			SourceCount = sourceCount;
			GlobalSelectedHandler = globalSelectedHandler;
		}

		public ISourceBlock<TGenome> Node(
			ISourceBlock<TGenome> source = null)
		{
			return GenomePipeline.Node(new ISourceBlock<TGenome>[] { source ?? Producer }, Problem, PoolSize, GlobalSelectedHandler);
		}

		IEnumerable<ISourceBlock<TGenome>> FirstLevelNodes()
		{
			for (byte i = 0; i < SourceCount; i++)
				yield return Node();
		}

		public ISourceBlock<TGenome> CreateNextLevelNode(IEnumerable<ISourceBlock<TGenome>> sources = null)
		{
			return GenomePipeline.Node(sources ?? FirstLevelNodes(), Problem, PoolSize, GlobalSelectedHandler);
		}

		// source node count = depth^SourceCount
		public ISourceBlock<TGenome> CreateNetwork(uint depth = 1)
		{
			if (depth == 0)
				throw new ArgumentOutOfRangeException("depth", depth, "Must be at least 1.");
			if (depth == 1) return Node();
			if (depth == 2) return CreateNextLevelNode();

			return CreateNextLevelNode(
				Enumerable.Range(0, (int)SourceCount)
					.Select(i => CreateNetwork(depth - 1))
			);
		}

		public ITargetBlock<TGenome>
			Distributor(
				Action<TGenome[]> selected,
				Action<TGenome[]> rejected = null)
		{
			return GenomePipeline.Distributor(Problem, PoolSize,
				new ActionBlock<TGenome[]>(selected),
				rejected == null ? null : new ActionBlock<TGenome[]>(rejected));
		}
	}

}