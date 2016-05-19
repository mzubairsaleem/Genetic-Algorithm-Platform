/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import Set from "typescript-dotnet/source/System/Collections/Set";
import StringKeyDictionary from "typescript-dotnet/source/System/Collections/Dictionaries/StringKeyDictionary";
import * as ArrayUtility from "typescript-dotnet/source/System/Collections/Array/Utility";
import {correlation} from "./arithmetic/Correlation";
import AlgebraGenome from "./Genome";
import AlgebraFitness from "./Fitness";
import Enumerable from "typescript-dotnet/source/System.Linq/Linq";
import Type from "typescript-dotnet/source/System/Types";
import Population from "../../source/Population";
import {IEnumerableOrArray} from "typescript-dotnet/source/System/Collections/IEnumerableOrArray";
import {IMap} from "typescript-dotnet/source/System/Collections/Dictionaries/IDictionary";
import {IProblem} from "../../source/IProblem";

function actualFormula(a:number, b:number):number // Solve for 'c'.
{
	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
}

export default class AlgebraBlackBoxProblem implements IProblem<AlgebraGenome, AlgebraFitness>
{
	private _fitness:IMap<AlgebraFitness>;
	private _actualFormula:(...params:number[])=>number;

	protected _convergent:StringKeyDictionary<AlgebraGenome>;
	get convergent():AlgebraGenome[] {
		return this._convergent.values;
	}

	constructor(actualFormula:(...params:number[])=>number)
	{
		this._fitness = {};
		this._actualFormula = actualFormula;
		this._convergent = new StringKeyDictionary<AlgebraGenome>();
	}

	protected getScoreFor(genome:string):number;
	protected getScoreFor(genome:AlgebraGenome):number;
	protected getScoreFor(genome:any):number
	{
		if(!genome) return 0;
		if(!Type.isString(genome)) genome = genome.hash;
		let s = this._fitness[genome];
		return s && s.score || 0;
	}

	getFitnessFor(genome:AlgebraGenome, createIfMissing:boolean = true):AlgebraFitness
	{
		var h = genome.hash, f = this._fitness, s = f[h];
		if(!s && createIfMissing) f[h] = s = new AlgebraFitness();
		return s;
	}

	rank(population:IEnumerableOrArray<AlgebraGenome>):Enumerable<AlgebraGenome>
	{
		return Enumerable
			.from(population)
			.orderByDescending(g=>this.getScoreFor(g));
	}

	rankAndReduce(
		population:IEnumerableOrArray<AlgebraGenome>,
		targetMaxPopulation:number):Enumerable<AlgebraGenome>
	{
		var lastValue:number;
		return this.rank(population)
			.takeWhile((g, i)=>
			{
				let lv = lastValue, s = this.getScoreFor(g);
				lastValue = s;
				return i<targetMaxPopulation || lv===s
			});
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


	test(p:Population<AlgebraGenome>, count:number = 1):void
	{
		for(let i = 0; i<count; i++)
		{
			var aSample = this.sample();
			var bSample = this.sample();
			var correct:number[] = [];

			for(let a of aSample)
			{
				for(let b of bSample)
				{
					correct.push(actualFormula(a, b));
				}
			}

			p.forEach(g=>
			{
				let result:number[] = [];
				for(let a of aSample)
				{
					for(let b of bSample)
					{
						result.push(g.calculate([a, b]));
					}
				}

				let c = correlation(correct, result);
				this.getFitnessFor(g)
					.add((isNaN(c) || !isFinite(c))?-2:c);
				this._convergent.setValue(g.hash,c==1?g:(void 0));
			});

		}
	}


}
