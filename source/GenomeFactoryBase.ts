///<reference path="IGenomeFactory.d.ts"/>
import Genome from "./Genome";
import LinkedList from "../node_modules/typescript-dotnet/source/System/Collections/LinkedList";
import ReadOnlyCollectionWrapper from "../node_modules/typescript-dotnet/source/System/Collections/ReadOnlyCollectionWrapper";

abstract class GenomeFactoryBase<TGenome extends Genome,TFitness>
implements IGenomeFactory<TGenome,TFitness>
{
	protected _previousGenomes:LinkedList<TGenome>;
	protected _previousGenomeWrapper:ReadOnlyCollectionWrapper<TGenome>;

	constructor(
		private _inputParamCount:number,
		public maxGenomeTracking:number = 1000)
	{
		this._previousGenomes = new LinkedList<TGenome>();
	}


	get inputParamCount():number
	{
		return this._inputParamCount;
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



	abstract generate():TGenome;
	abstract generateFrom(source:IEnumerableOrArray<Organism<TGenome,TFitness>>):TGenome;
	abstract mutate(source:TGenome, mutations?:number):TGenome;
}

export default GenomeFactoryBase;
