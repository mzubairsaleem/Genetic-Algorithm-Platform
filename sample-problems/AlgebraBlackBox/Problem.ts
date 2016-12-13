/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
import Set from "typescript-dotnet-umd/System/Collections/Set";
import StringKeyDictionary from "typescript-dotnet-umd/System/Collections/Dictionaries/StringKeyDictionary";
import {correlation} from "./arithmetic/Correlation";
import AlgebraGenome from "./Genome";
import Fitness from "../../source/Fitness";
import Enumerable from "typescript-dotnet-umd/System.Linq/Linq";
import Population from "../../source/Population";
import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";
import {IMap} from "typescript-dotnet-umd/System/Collections/Dictionaries/IDictionary";
import {IProblem} from "../../source/IProblem";
import {IOrderedEnumerable, ILinqEnumerable} from "typescript-dotnet-umd/System.Linq/Enumerable";
import {average} from "typescript-dotnet-umd/System/Collections/Array/Procedure";
import {Promise as NETPromise} from "typescript-dotnet-umd/System/Promises/Promise";

export default class AlgebraBlackBoxProblem implements IProblem<AlgebraGenome, Fitness>
{
	private _fitness:IMap<Fitness>;
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

	getFitnessFor(genome:AlgebraGenome, createIfMissing:boolean = true):Fitness
	{
		// Avoid repeating processes by using the reduced hash as a key.
		const h = genome.hashReduced, f = this._fitness;
		let s = f[h];
		if(!s && createIfMissing) f[h] = s = new Fitness();
		return s;
	}

	rank(population:IEnumerableOrArray<AlgebraGenome>):IOrderedEnumerable<AlgebraGenome>
	{
		return Enumerable(population)
			.orderByDescending(g => this.getFitnessFor(g))
			.thenBy(g => g.hash.length);
	}

	rankAndReduce(
		population:IEnumerableOrArray<AlgebraGenome>,
		targetMaxPopulation:number):ILinqEnumerable<AlgebraGenome>
	{
		let lastFitness:Fitness;
		return this.rank(population)
			.takeWhile((g, i) =>
			{
				let lf = lastFitness, f = this.getFitnessFor(g);
				lastFitness = f;
				return i<targetMaxPopulation || lf.compareTo(f)===0;
			});
	}

	//noinspection JSMethodCanBeStatic,JSUnusedGlobalSymbols
	async correlation(
		aSample:ArrayLike<number>, bSample:ArrayLike<number>, gA:AlgebraGenome,
		gB:AlgebraGenome):NETPromise<number>
	{
		const len = aSample.length*bSample.length;

		const gA_result = new Float64Array(len);
		const gB_result = new Float64Array(len);
		let i:number = 0;
		for(let a of <number[]>aSample)
		{
			for(let b of <number[]>bSample)
			{
				const params = [a, b];
				gA_result[i] = gA.calculate(params);
				gB_result[i] = gB.calculate(params);
				i++;
			}
		}

		return correlation(gA_result, gB_result);

	}


	// compare(a:AlgebraGenome, b:AlgebraGenome):boolean
	// {
	// 	return this.correlation(this.sample(), this.sample(), a, b)>0.9999999;
	// }


	//noinspection JSMethodCanBeStatic
	sample(count:number = 5, range:number = 100):number[]
	{
		const result = new Set<number>();

		while(result.count<count)
		{
			result.add(Math.random()*range);
		}
		const a = result.toArray();
		a.sort();
		return a;
	}


	async test(p:Population<AlgebraGenome>, count:number = 1):NETPromise<void>
	{
		// TODO: Need to find a way to dynamically implement more than 2 params... (discover significant params)
		let f = this._actualFormula;
		//noinspection JSMismatchedCollectionQueryUpdate
		const result:PromiseLike<any>[] = [];
		const genes = p.toArray();
		for(let i = 0; i<count; i++)
		{
			const aSample = this.sample();
			const bSample = this.sample();
			const correct:number[] = [];

			for(let a of aSample)
			{
				for(let b of bSample)
				{
					correct.push(f(a, b));
				}
			}

			for(let g of genes)
			{
				let calc:number[] = [];
				for(let a of aSample)
				{
					for(let b of bSample)
					{
						calc.push(g.calculate([a, b]));
					}
				}
				let divergence:number[] = [];
				let len = correct.length;
				divergence.length = correct.length;

				for(let i = 0; i<len; i++)
				{
					divergence[i] = -Math.abs(calc[i] - correct[i]);
				}

				let c = correlation(correct, calc);
				let d = average(divergence) + 1;

				let f = this.getFitnessFor(g);
				f.add([
					(isNaN(c) || !isFinite(c)) ? -2 : c,
					(isNaN(d) || !isFinite(d)) ? -Infinity : d
				]);

				this._convergent.setValue(g.hashReduced, f.hasConverged()
					? g
					: (void 0));

			}

		}

		await result;
	}


}

