///<reference path="IGenomeFactory.d.ts"/>
import Genome from "./Genome";
import LinkedList from "../node_modules/typescript-dotnet/source/System/Collections/LinkedList";
import ReadOnlyCollectionWrapper from "../node_modules/typescript-dotnet/source/System/Collections/ReadOnlyCollectionWrapper";

abstract class GenomeFactoryBase<TGenome extends Genome>
implements IGenomeFactory<TGenome>
{
	protected _previousGenomes:LinkedList<TGenome>;
	protected _previousGenomeWrapper:ReadOnlyCollectionWrapper<TGenome>;

	constructor(public maxGenomeTracking:number = 1000)
	{
		this._previousGenomes = new LinkedList<TGenome>();
	}

	get previousGenomes():ReadOnlyCollectionWrapper<TGenome>
	{
		return this._previousGenomeWrapper
			|| (this._previousGenomeWrapper = new ReadOnlyCollectionWrapper<TGenome>(this._previousGenomes));
	}

	trimPreviousGenomes():void
	{
		while(this._previousGenomes.count>this.maxGenomeTracking)
			this._previousGenomes.removeFirst();
	}


	abstract generate(inputParamCount:number, source?:IEnumerableOrArray<TGenome>):TGenome;
	abstract mutate(source:TGenome, mutations?:number):TGenome;
}

export default GenomeFactoryBase;
