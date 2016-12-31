/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GeneticAlgorithmPlatform
{

    // Defines the pipeline?
    public abstract class Environment<TGenome> : IEnvironment<TGenome>
		where TGenome : class, IGenome
	{
		IGenomeFactory<TGenome> Factory;

		BufferBlock<IProblem<TGenome>> Converged;

		protected Environment(IGenomeFactory<TGenome> genomeFactory)
		{
			Factory = genomeFactory;
		}

		public void AddProblem(IProblem<TGenome> problem)
		{
			problem.Consume(Factory);
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

	}


}
