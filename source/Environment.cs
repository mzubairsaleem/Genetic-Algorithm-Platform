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

		readonly BufferBlock<Tuple<IProblem<TGenome>,TGenome,Fitness>>
			TopChanges = new BufferBlock<Tuple<IProblem<TGenome>,TGenome,Fitness>>();

		protected Environment(IGenomeFactory<TGenome> genomeFactory)
		{
			Factory = genomeFactory;
		}

		public void AddProblem(IProblem<TGenome> problem)
		{
			problem.Consume(Factory);
			problem.ListenToTopChanges(new ActionBlock<Tuple<TGenome,Fitness>>(genomeAndFitness=>
				TopChanges.Post(new Tuple<IProblem<TGenome>,TGenome,Fitness>(problem,genomeAndFitness.Item1,genomeAndFitness.Item2))
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

		public IDisposable ListenToTopChanges(Action<Tuple<IProblem<TGenome>,TGenome,Fitness>> handler)
		{
			return ListenToTopChanges(new ActionBlock<Tuple<IProblem<TGenome>,TGenome,Fitness>>(handler));
		}
		public IDisposable ListenToTopChanges(ITargetBlock<Tuple<IProblem<TGenome>,TGenome,Fitness>> target)
		{
			return TopChanges.LinkTo(target);
		}

	}


}
