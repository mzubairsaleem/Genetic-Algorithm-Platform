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
			AddProblem(new Problem(actualFormula));
		}

	}

}