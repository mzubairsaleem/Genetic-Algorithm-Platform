using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

		public static IPropagatorBlock<TGenome, GenomeFitness<TGenome>[]>
		Processor<TGenome>(GenomeTestDelegate<TGenome> test, int size)
			where TGenome : IGenome
		{
			if (size < 2)
				throw new ArgumentOutOfRangeException("size", size, "Must be at least 2.");

			var input = new BatchBlock<TGenome>(size);
			var output = new TransformBlock<TGenome[], GenomeFitness<TGenome>[]>(async batch =>
			{
				long batchId = Interlocked.Increment(ref BatchID);
				return await Task.WhenAll(batch.Select(async g => new GenomeFitness<TGenome>(g, await test(g, batchId))))
					.ContinueWith(task => task.Result.OrderBy(g => g)
					.ToArray());
			});

			input.LinkTo(output);

			return DataflowBlock.Encapsulate(input, output);
		}

		public static TransformBlock<GenomeFitness<TGenome>[], GenomeSelection<TGenome>>
		Selector<TGenome>(IProblem<TGenome> problem)
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
				for (var i = 0; i < selectionPoint; i++)
					selected[i] = results[i].Genome;
				for (var i = selectionPoint; i < len; i++)
					selected[i - selectionPoint] = results[i].Genome;
				return new GenomeSelection<TGenome>(selected, rejected);
			});
		}

		public static IPropagatorBlock<TGenome, GenomeSelection<TGenome>>
		SelectionProcessor<TGenome>(IProblem<TGenome> problem, int size)
			where TGenome : IGenome
		{
			if (problem == null)
				throw new ArgumentNullException("problem");

			return DataflowBlock.Encapsulate(Processor(problem.TestProcessor, size), Selector(problem));
		}

		public static IPropagatorBlock<TGenome, GenomeSelection<TGenome>>
		Distributor<TGenome>(IProblem<TGenome> problem, int size, ITargetBlock<TGenome[]> selected, ITargetBlock<TGenome[]> rejected = null)
			where TGenome : IGenome
		{
			if (selected == null)
				throw new ArgumentNullException("selected");

			var input = SelectionProcessor(problem, size);
			input.LinkTo(new ActionBlock<GenomeSelection<TGenome>>(selection =>
			{
				selected.Post(selection.Selected);
				if (rejected != null) rejected.Post(selection.Rejected);
			}));
			return input;
		}

	}
}