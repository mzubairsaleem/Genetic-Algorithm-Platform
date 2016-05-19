/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


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