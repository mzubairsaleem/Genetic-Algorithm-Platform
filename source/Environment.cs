/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GeneticAlgorithmPlatform
{

	// Defines the pipeline?
	public abstract class Environment<TGenome> : IEnvironment<TGenome>
		where TGenome : class, IGenome
	{
		readonly BufferBlock<IProblem<TGenome>>
			Converged = new BufferBlock<IProblem<TGenome>>();

		readonly IGenomeFactory<TGenome> Factory;

		readonly BufferBlock<Tuple<IProblem<TGenome>,IGenomeFitness<TGenome>>>
			TopChanges = new BufferBlock<Tuple<IProblem<TGenome>,IGenomeFitness<TGenome>>>();

		protected Environment(IGenomeFactory<TGenome> genomeFactory)
		{
			Factory = genomeFactory;
		}

		public void AddProblem(IProblem<TGenome> problem)
		{
			problem.Consume(Factory);
			problem.ListenToTopChanges(new ActionBlock<IGenomeFitness<TGenome>>(gf=>
				TopChanges.Post(new Tuple<IProblem<TGenome>,IGenomeFitness<TGenome>>(problem,gf))
			));
			problem.WaitForConverged()
				.ContinueWith(task => Converged.Post(problem));
		}

		public async Task<IList<IProblem<TGenome>>> WaitForConverged()
		{
			await Converged.OutputAvailableAsync();
			IList<IProblem<TGenome>> list;
			Converged.TryReceiveAll(out list);
			return list;
		}

		public void SpawnNew(uint count = 1)
		{
			Factory.Generate(count);
		}

		public IDisposable ListenToTopChanges(Action<Tuple<IProblem<TGenome>,IGenomeFitness<TGenome>>> handler)
		{
			return ListenToTopChanges(new ActionBlock<Tuple<IProblem<TGenome>,IGenomeFitness<TGenome>>>(handler));
		}
		public IDisposable ListenToTopChanges(ITargetBlock<Tuple<IProblem<TGenome>,IGenomeFitness<TGenome>>> target)
		{
			return TopChanges.LinkTo(target);
		}

	}


}
