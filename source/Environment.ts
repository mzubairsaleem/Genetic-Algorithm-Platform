/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import * as Triangular from "./Triangular";
import {dispose} from "typescript-dotnet/source/System/Disposable/dispose";
import {LinkedList} from "typescript-dotnet/source/System/Collections/LinkedList";
import {TaskHandlerBase} from "typescript-dotnet/source/System/Threading/Tasks/TaskHandlerBase";
import {Population} from "./Population";
import {Enumerable} from "typescript-dotnet/source/System.Linq/Linq";
import {IEnumerable} from "typescript-dotnet/source/System/Collections/Enumeration/IEnumerable";
import {IEnumerableOrArray} from "typescript-dotnet/source/System/Collections/IEnumerableOrArray";
import {IGenome} from "./IGenome";
import {IEnvironment} from "./IEnvironment";
import {IProblem} from "./IProblem";
import {IGenomeFactory} from "./IGenomeFactory";
import Stopwatch from "typescript-dotnet/source/System/Diagnostics/Stopwatch";

export class Environment<TGenome extends IGenome>
extends TaskHandlerBase implements IEnvironment<TGenome>
{

	protected _generations:number = 0;
	protected _populations:LinkedList<Population<TGenome>>;
	protected _problems:IProblem<TGenome,any>[];
	protected _problemsEnumerable:Enumerable<IProblem<TGenome,any>>;

	populationSize:number = 50;
	maxPopulations:number = 10;
	testCount:number = 5;


	constructor(private _genomeFactory:IGenomeFactory<TGenome>)
	{
		super();
		this._problemsEnumerable
			= Enumerable.from(this._problems = []);
		this._populations = new LinkedList<Population<TGenome>>();
	}

	test(count:number = this.testCount):void
	{
		let p = this._populations;
		for(let pr of this._problems)
		{
			p.forEach(po=>pr.test(po, count));
		}
	}

	get generations():number
	{
		return this._generations;
	}

	get populations():number
	{
		return this._populations.count;
	}

	protected _onExecute():void
	{
		var populations = this._populations.linq.reverse(),
		    problems    = this._problemsEnumerable.memoize();

		// Get ranked population for each problem and merge it into a weaved enumeration.
		var sw = Stopwatch.startNew();
		var p = this.spawn(
			this.populationSize,
			Triangular.disperse.decreasing<TGenome>(
				Enumerable.weave<TGenome>(populations
					.selectMany<IEnumerable<TGenome>>(
						o => {
							let x = problems.select(r=>r.rank(o));
							if(!x.any()) return x;
							return Enumerable.make(x.first()).concat(x); // Take the first one an bias it as the winner.
						}
					)
				)
			)
		);
		console.log("Populations:",this._populations.count);
		console.log("Selection/Ranking (ms):",sw.currentLapMilliseconds);
		sw.lap();

		this.test();
		this._generations++;

		p.keepOnly(
			Enumerable.weave<TGenome>(
				problems.select(r=>r.rank(p)))
				.take(this.populationSize/2));

		dispose(populations);
		console.log("Testing/Cleanup (ms):",sw.currentLapMilliseconds);

	}


	/**
	 * Adds a new population to the environment.  Optionally pulling from the source provided.
	 */
	spawn(populationSize:number, source?:IEnumerableOrArray<TGenome>):Population<TGenome>
	{
		var _ = this;
		var p = new Population(_._genomeFactory);

		p.populate(populationSize, Enumerable.from(source).toArray());

		_._populations.add(p);
		_._genomeFactory.trimPreviousGenomes();
		_.trimEarlyPopulations(_.maxPopulations);
		return p;
	}


	trimEarlyPopulations(maxPopulations:number):void
	{
		var problems = this._problemsEnumerable.memoize(), pops = this._populations;
		pops.linq
			.takeExceptLast(maxPopulations)
			.forEach(p=>
			{
				// Move top items to latest population.
				problems.forEach(r=>{
					var keep = Enumerable.from(r.rank(p)).firstOrDefault();
					if(keep) pops.last.value.add(keep);
				});

				pops.remove(p);
			});
	}

}

export default Environment;