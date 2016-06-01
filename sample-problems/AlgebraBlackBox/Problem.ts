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
import Population from "../../source/Population";
import {IEnumerableOrArray} from "typescript-dotnet/source/System/Collections/IEnumerableOrArray";
import {IMap} from "typescript-dotnet/source/System/Collections/Dictionaries/IDictionary";
import {IProblem} from "../../source/IProblem";
import {IOrderedEnumerable, ILinqEnumerable} from "typescript-dotnet/source/System.Linq/Enumerable";

export default class AlgebraBlackBoxProblem implements IProblem<AlgebraGenome, AlgebraFitness>
{
	private _fitness:IMap<AlgebraFitness>;
	private _actualFormula:(...params:number[])=>number;

	protected _convergent:StringKeyDictionary<AlgebraGenome>;
	get convergent():AlgebraGenome[]
	{
		return this._convergent.values;
	}

	constructor(actualFormula:(...params:number[])=>number)
	{
		this._fitness = {};
		this._actualFormula = actualFormula;
		this._convergent = new StringKeyDictionary<AlgebraGenome>();
	}

	// protected getScoreFor(genome:string):number;
	// protected getScoreFor(genome:AlgebraGenome):number;
	// protected getScoreFor(genome:any):number
	// {
	// 	if(!genome) return 0;
	// 	if(!Type.isString(genome)) genome = genome.hashReduced;
	// 	let s = this._fitness[genome];
	// 	return s && s.score || 0;
	// }

	getFitnessFor(genome:AlgebraGenome, createIfMissing:boolean = true):AlgebraFitness
	{
		// Avoid repeating processes by using the reduced hash as a key.
		var h = genome.hashReduced, f = this._fitness, s = f[h];
		if(!s && createIfMissing) f[h] = s = new AlgebraFitness();
		return s;
	}

	rank(population:IEnumerableOrArray<AlgebraGenome>):IOrderedEnumerable<AlgebraGenome>
	{
		return Enumerable
			.from(population)
			.orderByDescending(g=>this.getFitnessFor(g))
			.thenBy(g=>g.hash.length);
	}

	rankAndReduce(
		population:IEnumerableOrArray<AlgebraGenome>,
		targetMaxPopulation:number):ILinqEnumerable<AlgebraGenome>
	{
		var lastFitness:AlgebraFitness;
		return this.rank(population)
			.takeWhile((g, i)=>
			{
				let lf = lastFitness, f = this.getFitnessFor(g);
				lastFitness = f;
				return i<targetMaxPopulation || lf.compareTo(f)===0;
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
		let f = this._actualFormula;
		for(let i = 0; i<count; i++)
		{
			const aSample = this.sample();
			const bSample = this.sample();
			const correct:number[] = [];
			const flat:number[] = [];

			for(let a of aSample)
			{
				for(let b of bSample)
				{
					correct.push(f(a, b));
					flat.push(0);
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
				let divergence:number[] = [];
				let len = correct.length;
				divergence.length = correct.length;

				for(let i = 0; i<len; i++)
				{
					divergence[i] = result[i] = correct[i];
				}

				let c = correlation(correct, result);
				let d = correlation(flat, divergence);

				let f = this.getFitnessFor(g);
				f.add([
					(isNaN(c) || !isFinite(c)) ? -2 : c,
					(isNaN(d) || !isFinite(d)) ? -2 : d
				]);

				this._convergent.setValue(g.hash, f.hasConverged ? g : (void 0));
			});

		}
	}


}

