/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

import * as ArrayUtility from "../node_modules/typescript-dotnet/source/System/Collections/Array/Utility";

function actualFormula(a:number, b:number):number // Solve for 'c'.
{
	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
}

class AlgebraBlackBox implements IProblem
{
	getGeneFactory():GeneFactory
	{
		return new GeneFactory(2);
	}

	correlation(aSample:number[], bSample:number[], gA:Genome, gB:Genome):number
	{
		var len = aSample.length*bSample.length;

		var gA_result = ArrayUtility.initialize<number>(len);
		var gB_result = ArrayUtility.initialize<number>(len);
		for(var a of aSample)
		{
			for(var b of bSample)
			{
				gA_result.push(gA.Root.Calculate([a, b]));
				gB_result.push(gB.Root.Calculate([a, b]));
			}
		}

		return gA_result.Correlation(gB_result);

	}


	compare(a:Genome, b:Genome):boolean
	{
		return Correlation(Sample(), Sample(), a, b)>(1 - number.Epsilon);
	}


	sample(count:number = 5, range:number = 100)
	{
		var result = new HashSet<number>();

		while(result.Count<count)
		{
			result.Add(Math.random()*range);
		}
		return result.OrderBy(v=>v).ToArray();
	}


	test(p:Population, count:number = 1)
	{
		for(var i = 0; i<count; i++)
		{
			var aSample = this.sample();
			var bSample = this.sample();
			var len = aSample.Length*bSample.Length;
			var correct = ArrayUtility.initialize<number>(len);

			for(var a of aSample)
			{
				for(var b of bSample)
				{
					correct.Add(actualFormula(a, b));
				}
			}

			for(var o of p)
			{
				var result = ArrayUtility.initialize<number>(len);
				for(var a of aSample)
				{
					for(var b of bSample)
					{
						result.push(o.Genome.Root.Calculate([a, b]));
					}
				}


				o.AddFitness(correct.Correlation(result));
			}

		}
	}



}
