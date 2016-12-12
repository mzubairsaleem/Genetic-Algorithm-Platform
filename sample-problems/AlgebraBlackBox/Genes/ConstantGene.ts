/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import AlgebraGene from "../Gene";
import UnreducibleGene from "./UnreducibleGene";

const EMPTY = "";

export default class ConstantGene extends UnreducibleGene
{
	//protected
	toStringInternal():string
	{
		return this._multiple + EMPTY;
	}

	toStringContents():string
	{
		return EMPTY;
	}

	clone():ConstantGene
	{
		return new ConstantGene(this._multiple);
	}

	//noinspection JSUnusedLocalSymbols
	calculateWithoutMultiple(values:ArrayLike<number>):number
	{
		return 1;
	}

	equals(other:AlgebraGene):boolean
	{
		return other instanceof ConstantGene && this._multiple==other._multiple || super.equals(other);
	}
}
