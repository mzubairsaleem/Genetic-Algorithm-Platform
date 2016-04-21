/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

import Set from "../node_modules/typescript-dotnet/source/System/Collections/Set";
import * as ArrayUtility from "../node_modules/typescript-dotnet/source/System/Collections/Array/Utility";
import {correlation} from "../source/arithmetic/Correlation";
import AlgebraGene from "./AlgebraGenome";
import AlgebraGenome from "./AlgebraGenome";
import AlgebraFitness from "./AlgebraFitness";
import AlgebraGenomeFactory from "./AlgebraGenomeFactory";
import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";
import Population from "../source/Population";

function actualFormula(a:number, b:number):number // Solve for 'c'.
{
	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
}

export default class AlgebraBlackBoxProblem implements IProblem<AlgebraGene,AlgebraFitness>
{
	rank(population:IEnumerableOrArray<IOrganism<AlgebraGenome, AlgebraFitness>>):Enumerable<IOrganism<AlgebraGenome, AlgebraFitness>>
	{
		return Enumerable
			.from(population)
			.orderByDescending(o=>o.fitness.score);
	}

	rankAndReduce(
		population:IEnumerableOrArray<IOrganism<AlgebraGenome, AlgebraFitness>>,
		targetMaxPopulation:number):Enumerable<IOrganism<AlgebraGenome, AlgebraFitness>>
	{
		var lastValue:number;
		return this.rank(population)
			.takeWhile((o, i)=>
			{
				var lv = lastValue, s = o.fitness.score;
				lastValue = s;
				return i<targetMaxPopulation || lv===s
			});
	}

	getGenomeFactory():AlgebraGenomeFactory
	{
		return new AlgebraGenomeFactory(2);
	}

	correlation(aSample:number[], bSample:number[], gA:AlgebraGenome, gB:AlgebraGenome):number
	{
		var len = aSample.length*bSample.length;

		var gA_result = ArrayUtility.initialize<number>(len);
		var gB_result = ArrayUtility.initialize<number>(len);
		for(var a of aSample)
		{
			for(var b of bSample)
			{
				gA_result.push(gA.calculate([a, b]));
				gB_result.push(gB.calculate([a, b]));
			}
		}

		return correlation(gA_result, gB_result);

	}


	// compare(a:AlgebraGenome, b:AlgebraGenome):boolean
	// {
	// 	return this.correlation(this.sample(), this.sample(), a, b)>0.9999999;
	// }


	sample(count:number = 5, range:number = 100):number[]
	{
		var result = new Set<number>();

		while(result.count<count)
		{
			result.add(Math.random()*range);
		}
		var a = result.toArray();
		a.sort();
		return a;
	}


	test(p:Population<AlgebraGene,AlgebraFitness>, count:number = 1)
	{
		for(var i = 0; i<count; i++)
		{
			var aSample = this.sample();
			var bSample = this.sample();
			var correct:number[] = [];

			for(var a of aSample)
			{
				for(var b of bSample)
				{
					correct.push(actualFormula(a, b));
				}
			}

			p.forEach(o=>
			{
				var result:number[] = [];
				for(var a of aSample)
				{
					for(var b of bSample)
					{
						result.push(o.genome.calculate([a, b]));
					}
				}

				o.fitness.add(correlation(correct, result));
			});

		}
	}


}
