using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlgebraBlackBox
{

    // function actualFormula(a:number, b:number):number // Solve for 'c'.
    // {
    // 	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2) + a) + b;
    // }

    public class Environment : GeneticAlgorithmPlatform.Environment<Genome>
	{

		public Environment(Formula actualFormula) : base(new GenomeFactory())
		{
			_problems
				.Add(new Problem(actualFormula));
			
			Task.Run(async ()=>{

				while(true)
				{
					Spawn();
					await Task.Delay(1000);
					foreach(var p in _problems)
					{
						Console.WriteLine(p.PeekNextTopGenome());
					}
				}
			});
		}

		public async Task<Genome> WaitForAnyConvergedAsync()
		{
			return await await Task.WhenAny(_problems.Select(p=>p.WaitForConverged()));
		}

		public Genome WaitForAnyConverged()
		{
			var gt = WaitForAnyConvergedAsync();
			gt.Wait();
			return gt.Result;
		}

	}



}
