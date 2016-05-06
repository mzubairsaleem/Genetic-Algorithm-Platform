import Integer from "../../../node_modules/typescript-dotnet/source/System/Integer";
import AlgebraGene from "../Gene";

export default class ParameterGene extends AlgebraGene implements IEquatable<ParameterGene>
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

	asReduced():ParameterGene
	{

		return this.clone();

	}

	equals(other:ParameterGene):boolean
	{
		return this._id==other._id && this._multiple==other._multiple;
	}
}
