///<reference path="IEnvironment.d.ts"/>
import LinkedList from "../node_modules/typescript-dotnet/source/System/Collections/LinkedList";
import TaskHandlerBase from "../node_modules/typescript-dotnet/source/System/Tasks/TaskHandlerBase";
import Population from "./Population";
import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";
import * as Triangular from "./Triangular";

export default class Environment<TGenome extends IGenome>
extends TaskHandlerBase implements IEnvironment<TGenome>
{

	protected _populations:LinkedList<Population<TGenome>>;
	protected _problems:IProblem<TGenome,any>[];

	populationSize:number = 50;
	testCount:number = 10;

	constructor(private _genomeFactory:IGenomeFactory<TGenome>)
	{
		super();
		this._problems = [];
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

	get populations():number
	{
		return this._populations.count;
	}

	protected _onExecute():void
	{
		var populations = Enumerable.from(this._populations).reverse(),
		    problems    = Enumerable.from(this._problems).memoize();

		// Get ranked population for each problem and merge it into a weaved enumeration.
		var p = this.spawn(
			this.populationSize,
			Triangular.dispurse.decreasing<TGenome>(
				Enumerable.weave<TGenome>(populations
					.selectMany<IEnumerable<TGenome>>(
						o => problems.select(r=>r.rank(o))
					)
				)
			)
		);

		this.test();

		p.keepOnly(
			Enumerable.weave<TGenome>(
				problems.select(r=>r.rank(p)))
			.take(this.populationSize/2));

		this.execute(0);
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
		_.trimEarlyPopulations(10);
		return p;
	}


	trimEarlyPopulations(maxPopulations:number):void
	{
		var p = this._populations;
		while(p.count>maxPopulations)
		{
			p.removeFirst();
		}
	}

}
