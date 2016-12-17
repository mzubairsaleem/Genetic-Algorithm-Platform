/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {OrderedStringKeyDictionary} from "typescript-dotnet-umd/System/Collections/Dictionaries/OrderedStringKeyDictionary";
import {IGenome} from "./IGenome";
import {IGenomeFactory} from "./IGenomeFactory";

export abstract class GenomeFactoryBase<TGenome extends IGenome<any>>
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

	getPrevious(hash:string):TGenome|undefined
	{
		return this._previousGenomes.getValue(hash);
	}

	trimPreviousGenomes():void
	{
		while(this._previousGenomes.count>this.maxGenomeTracking)
		{
			this._previousGenomes.removeByIndex(0);
		}
	}

	abstract generateVariations(source:TGenome):TGenome[];

	abstract generate(source?:TGenome[]):TGenome|null;

	abstract mutate(source:TGenome, mutations?:number):TGenome;
}

export default GenomeFactoryBase;
