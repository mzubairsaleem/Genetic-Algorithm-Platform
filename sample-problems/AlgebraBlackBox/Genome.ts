import Genome from "../../source/Genome";
import InvalidOperationException from "typescript-dotnet-umd/System/Exceptions/InvalidOperationException";
import AlgebraGene from "./Gene";

export default class AlgebraGenome extends Genome<AlgebraGene>
{

	constructor(root?:AlgebraGene)
	{
		super(root);
	}

	clone():AlgebraGenome
	{
		return new AlgebraGenome(this.root.clone());
	}

	serialize():string
	{
		var root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.serialize();
	}

	serializeReduced():string
	{
		var root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.asReduced().serialize();
	}

	private _hashReduced:string;
	get hashReduced():string {
		return this._hashReduced || (this._hashReduced = this.serializeReduced());
	}

	resetHash():void {
		super.resetHash();
		this._hashReduced = null;
	}


	calculate(values:number[]):number {
		var root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.calculate(values);
	}
}
