import AlgebraGene from "../Gene";

const EMPTY = "";

export default class ConstantGene extends AlgebraGene implements IEquatable<ConstantGene>
{
	
	protected toStringInternal():string
	{
		return this.multiple + EMPTY;
	}

	toStringContents():string
	{
		return EMPTY;
	}

	clone():ConstantGene
	{
		return new ConstantGene(this.multiple);
	}

	calculateWithoutMultiple(values:number[]):number
	{
		return 1;
	}

	asReduced():ConstantGene
	{
		return this.clone();
	}
	
	equals(other:ConstantGene):boolean
	{
		return this.multiple==other.multiple;
	}
}
