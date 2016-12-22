import GeneBase from "../../source/GeneBase";
import {Enumerable} from "typescript-dotnet-umd/System.Linq/Linq";
import {supplant} from "typescript-dotnet-umd/System/Text/Utility";

const EMPTY:string = "";
const VARIABLE_NAMES = Object.freeze(Enumerable("abcdefghijklmnopqrstuvwxyz").toArray());

export function toAlphaParameters(hash:string):string
{
	return supplant(hash, VARIABLE_NAMES);
}


abstract class AlgebraGene extends GeneBase<AlgebraGene>
{

	constructor(protected _multiple:number = 1)
	{
		super();
	}

	abstract clone():AlgebraGene;

	abstract isReducible():boolean;

	abstract asReduced():AlgebraGene;

	serialize():string
	{
		return this.toString();
	}

	get multiple():number
	{
		return this._multiple;
	}

	set multiple(value:number)
	{
		if(this._multiple != value) {
			this._multiple = value;
			this._onModified();
		}
	}

	protected get multiplePrefix():string
	{
		const m = this._multiple;
		if(m!=1)
			return m== -1 ? "-" : (m + EMPTY);

		return EMPTY;
	}

	abstract toStringContents():string;

	protected toStringInternal():string
	{
		return this.multiplePrefix
			+ this.toStringContents();
	}

	toAlphaParameters():string
	{
		return toAlphaParameters(this.toString());
	}

	calculate(values:ArrayLike<number>):number
	{
		return this._multiple
			*this.calculateWithoutMultiple(values);
	}

	toEntityWithoutMultiple():string
	{
		return this.toStringContents();
	}

	toEntity():string {
		if(this.multiple==0) return "0";
		let prefix = this.multiplePrefix;
		let suffix = this.toEntityWithoutMultiple();
		if(prefix && suffix) return (prefix=="-" ? prefix : (prefix + "*")) + suffix;
		if(suffix) return suffix;
		if(prefix) return prefix;
		throw "No entity.";
	}

	protected abstract calculateWithoutMultiple(values:ArrayLike<number>):number;
}

export default AlgebraGene;
