import Type from "../node_modules/typescript-dotnet/source/System/Types";
import Integer from "../node_modules/typescript-dotnet/source/System/Integer";
import Set from "../node_modules/typescript-dotnet/source/System/Collections/Set";
import LinkedList from "../node_modules/typescript-dotnet/source/System/Collections/LinkedList";
import ArgumentOutOfRangeException from "../node_modules/typescript-dotnet/source/System/Exceptions/ArgumentOutOfRangeException";
import Genome from "./Genome";
import Population from "./Population";
import Organism from "./Organism";


export default class Environment<TGenome extends Genome,TFitness> implements IEnvironment
{
	static nextRandomIntegerExcluding(
		range:number,
		excluded:number|IEnumerableOrArray<number>):number
	{
		Integer.assert(range);
		if(range<0) throw new ArgumentOutOfRangeException("range", range, "Must be a number greater than zero.");

		var r:number[] = [],
		    excludeSet = new Set<number>(Type.isNumber(excluded, true) ? [excluded] : excluded);

		for(let i = 0; i<range; ++i)
		{
			if(!excludeSet.contains(i)) r.push(i);
		}

		return Integer.random.select(r);
	}

	trimEarlyPopulations(maxPopulations:number):void
	{
		var p = this._populations;
		while(p.count>maxPopulations)
		{
			p.removeFirst();
		}
	}

	private _genomeFactory:IGenomeFactory<TGenome,TFitness>;

	constructor(private _problem:IProblem<TGenome,TFitness>)
	{
		this._populations = new LinkedList<Population<TGenome,TFitness>>();
		this._genomeFactory = _problem.getGenomeFactory();
	}


	/**
	 * Runs a test cycle on the current population using the specified problem.
	 */
	spawn(populationSize:number):Population<TGenome,TFitness>
	{
		var _ = this;
		var p = new Population(_._genomeFactory);
		p.populate(populationSize);
		_._populations.add(p);
		_._genomeFactory.trimPreviousGenomes();
		_.trimEarlyPopulations(10);
		return p;
	}

	/**
	 * Runs a test cycle on the current population using the specified problem.
	 */
	spawnFrom(
		source:IEnumerableOrArray<Organism<TGenome,TFitness>>,
		populationSize:number):Population<TGenome,TFitness>
	{
		var _ = this;
		var p = new Population(_._genomeFactory);
		p.populateFrom(source, populationSize);
		_._populations.add(p);
		_._genomeFactory.trimPreviousGenomes();
		_.trimEarlyPopulations(10);
		return p;
	}


	get problem():IProblem<TGenome,TFitness>
	{
		return this._problem;
	}

	protected _populations:LinkedList<Population<TGenome,TFitness>>;


}
