/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
import Integer from "typescript-dotnet-umd/System/Integer";
import AlgebraGene from "../Gene";
import UnreducibleGene from "./UnreducibleGene";
import {Type} from "typescript-dotnet-umd/System/Types";
import {Regex} from "typescript-dotnet-umd/System/Text/RegularExpressions";

const PATTERN = new Regex("(?<multiple>-?\\d*){(?<id>\\d+)}");

export default class ParameterGene extends UnreducibleGene
{
	constructor(pattern:string);
	constructor(id:number, multiple?:number)
	constructor(id:number|string, multiple:number = 1)
	{
		if(Type.isString(id))
		{
			const m = PATTERN.match(id);
			if(!m)
				throw "Unrecognized parameter pattern.";
			const groups = m.namedGroups;
			let pm = groups["multiple"].value;
			if(pm) {
				if(pm==="" || pm==="-")
					pm += "1";
				multiple *= Number(pm);
			}

			id = Number(groups["id"].value);
		}

		super(multiple);
		Integer.assert(id, 'id');
		this._id = id;
	}

	protected _id:number;
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

	calculateWithoutMultiple(values:ArrayLike<number>):number
	{
		return values[this._id];
	}

	equals(other:AlgebraGene):boolean
	{
		return other==this || other instanceof ParameterGene && this._id==other._id && this._multiple==other._multiple || super.equals(other);
	}
}
