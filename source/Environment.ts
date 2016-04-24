///<reference path="IEnvironment.d.ts"/>
import LinkedList from "../node_modules/typescript-dotnet/source/System/Collections/LinkedList";
import Genome from "./Genome";
import Population from "./Population";


export default class Environment<TGenome extends Genome> implements IEnvironment<TGenome>
{
	protected _populations:LinkedList<Population<TGenome>>;
	protected _problems:IProblem<TGenome,any>[];

	constructor(private _genomeFactory:IGenomeFactory<TGenome>)
	{
		this._problems = [];
		this._populations = new LinkedList<Population<TGenome>>();
	}

	test():void {
		for(let pr of this._problems)
			this._populations.forEach(po=>pr.test(po));
	}


	/**
	 * Adds a new population to the environment.  Optionally pulling from the source provided.
	 */
	spawn(populationSize:number, source?:IEnumerableOrArray<TGenome>):Population<TGenome>
	{
		var _ = this;
		var p = new Population(_._genomeFactory);

		source ? p.populateFrom(source, populationSize) : p.populate(populationSize);
		
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
