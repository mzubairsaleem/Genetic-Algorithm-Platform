/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
import * as Triangular from "./Triangular";
import {dispose} from "typescript-dotnet-umd/System/Disposable/dispose";
import {LinkedList} from "typescript-dotnet-umd/System/Collections/LinkedList";
import {TaskHandlerBase} from "typescript-dotnet-umd/System/Threading/Tasks/TaskHandlerBase";
import {Population} from "./Population";
import {Enumerable, LinqEnumerable} from "typescript-dotnet-umd/System.Linq/Linq";
import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";
import {IGenome} from "./IGenome";
import {IEnvironment} from "./IEnvironment";
import {IProblem} from "./IProblem";
import {IGenomeFactory} from "./IGenomeFactory";
import {Promise as NETPromise} from "typescript-dotnet-umd/System/Promises/Promise";
import Stopwatch from "typescript-dotnet-umd/System/Diagnostics/Stopwatch";

export class Environment<TGenome extends IGenome>
extends TaskHandlerBase implements IEnvironment<TGenome>
{

	protected _generations:number = 0;
	protected _populations:LinkedList<Population<TGenome>>;
	protected _problems:IProblem<TGenome,any>[];
	protected _problemsEnumerable:LinqEnumerable<IProblem<TGenome,any>>;

	populationSize:number = 50;
	maxPopulations:number = 10;
	testCount:number = 5;


	constructor(private _genomeFactory:IGenomeFactory<TGenome>)
	{
		super();
		this._problemsEnumerable
			= Enumerable(this._problems = []);
		this._populations = new LinkedList<Population<TGenome>>();
	}

	async test(count:number = this.testCount):NETPromise<void>
	{
		let p = this._populations.toArray();
		let results = this._problems.map(problem =>
		{
			let calcP = p.map(population => problem.test(population, count));
			let a = NETPromise.all(calcP);
			a.delayAfterResolve(10).then(() => dispose.these(<any>calcP));
			return a;
		});

		let result = NETPromise.all(results);
		await result;
		dispose.these(results);
	}

	//noinspection JSUnusedGlobalSymbols
	get generations():number
	{
		return this._generations;
	}

	//noinspection JSUnusedGlobalSymbols
	get populations():number
	{
		return this._populations.count;
	}

	private _totalTime:number = 0;

	protected async _onAsyncExecute():NETPromise<void>
	{
		const sw = Stopwatch.startNew();
		const populations = this._populations.linq.reverse(),
		      problems    = this._problemsEnumerable.memoize();

		sw.lap();

		const allGenes = populations.selectMany(g=>g).memoize();
		// Get ranked population for each problem and merge it into a weaved enumeration.
		const previousP = problems.select(r => r.rank(allGenes));

		const p = this.spawn(
			this.populationSize, previousP.any() ?
				Triangular.disperse.decreasing(
					Enumerable.weave(previousP)
				) : void 0
		);

		const beforeCulling = p.count;
		if(!beforeCulling) // Just in case.
			throw "Nothing spawned!!!";

		// Retain genomes on the pareto...
		p.importEntries(problems.selectMany(r => r.pareto(allGenes)));

		console.log("Populations:", this._populations.count);
		console.log("Selection/Ranking (ms):", sw.currentLapMilliseconds);
		sw.lap();

		await this.test();
		this._generations++;

		// Since we have 'variations' added into the pool, we don't want to eliminate any new material that may be useful.
		const additional = Math.max(p.count - this.populationSize, 0);

		p.keepOnly(
			Enumerable.weave<TGenome>(
				problems.select(r => r.rank(p)))
				.take(this.populationSize/2 + additional));
		console.log("Population Size:", p.count, '/', beforeCulling);

		dispose(populations);
		console.log("Testing/Cleanup (ms):", sw.currentLapMilliseconds);
		const time = sw.elapsedMilliseconds;
		this._totalTime += time;
		console.log(
			"Generations:", this._generations+",",
			"Time:", time, "current /", this._totalTime, "total",
			"("+ Math.floor(this._totalTime/this._generations), "average)");
	}

	protected _onExecute():void
	{
		this._onAsyncExecute();
	}


	/**
	 * Adds a new population to the environment.  Optionally pulling from the source provided.
	 */
	spawn(populationSize:number, source?:IEnumerableOrArray<TGenome>):Population<TGenome>
	{
		const _ = this;
		const p = new Population(_._genomeFactory);

		p.populate(populationSize, source && Enumerable(source).toArray());

		_._populations.add(p);
		_._genomeFactory.trimPreviousGenomes();
		_.trimEarlyPopulations(_.maxPopulations);
		return p;
	}


	trimEarlyPopulations(maxPopulations:number):void
	{
		const problems = this._problemsEnumerable.memoize(), pops = this._populations;
		pops.linq
			.takeExceptLast(maxPopulations)
			.forEach(p =>
			{
				// Move top items to latest population.
				problems.forEach(r =>
				{
					const keep = Enumerable(r.rank(p)).firstOrDefault();
					if(keep) pops.last!.value.add(keep);
				});

				pops.remove(p);
			});
	}

}

export default Environment;