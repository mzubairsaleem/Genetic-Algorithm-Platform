import Genome from "../../source/Genome";
import InvalidOperationException from "../../node_modules/typescript-dotnet/source/System/Exceptions/InvalidOperationException";
import AlgebraGene from "./Gene";

export default class AlgebraGenome extends Genome<AlgebraGene>
{

	constructor(root?:AlgebraGene)
	{
		super(root);
	}

	clone():AlgebraGenome
	{
		return new AlgebraGenome(this.root);
	}

	serialize():string
	{
		var root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.serialize();
	}

	calculate(values:number[]):number {
		var root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.calculate(values);
	}
}
