///<reference path="IGenomeFactory.d.ts"/>
///<reference path="IGenome.d.ts"/>
import OrderedStringKeyDictionary from "../node_modules/typescript-dotnet/source/System/Collections/Dictionaries/OrderedStringKeyDictionary";

abstract class GenomeFactoryBase<TGenome extends IGenome>
implements IGenomeFactory<TGenome>
{
	protected _previousGenomes:OrderedStringKeyDictionary<TGenome>; // Track by hash...

	constructor(public maxGenomeTracking:number = 10000)
	{
		this._previousGenomes = new OrderedStringKeyDictionary<TGenome>();
	}

	get previousGenomes():string[]
	{
		return this._previousGenomes.keys;
	}

	getPrevious(hash:string):TGenome {
		return this._previousGenomes.getValue(hash);
	}

	trimPreviousGenomes():void
	{
		while(this._previousGenomes.count>this.maxGenomeTracking)
			this._previousGenomes.removeByIndex(0);
	}


	abstract generate(source?:TGenome[]):TGenome;
	abstract mutate(source:TGenome, mutations?:number):TGenome;
}

export default GenomeFactoryBase;
