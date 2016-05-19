/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import Integer from "typescript-dotnet/source/System/Integer";
import AlgebraGene from "../Gene";
import UnreducibleGene from "./UnreducibleGene";

export default class ParameterGene extends UnreducibleGene
{
	constructor(protected _id:number, multiple:number = 1)
	{
		super(multiple);
		Integer.assert(_id, 'id');
	}

	get id():number
	{
		return this._id;
	}


	toStringContents():string
	{
		return "{" + this._id + "}";
	}

	clone():ParameterGene
	{
		return new ParameterGene(this._id, this._multiple);
	}

	calculateWithoutMultiple(values:number[]):number
	{
		return values[this._id];
	}

	equals(other:AlgebraGene):boolean
	{
		return other==this || other instanceof ParameterGene && this._id==other._id && this._multiple==other._multiple || super.equals(other);
	}
}
