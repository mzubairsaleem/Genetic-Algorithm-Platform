import AlgebraGene from "../Gene";

abstract class UnreducibleGene extends AlgebraGene
{

	isReducible():boolean
	{
		return false;
	}

	asReduced():AlgebraGene
	{
		return this;
	}

}

export default UnreducibleGene;