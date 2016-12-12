import Genome from "../../source/Genome";
import InvalidOperationException from "typescript-dotnet-umd/System/Exceptions/InvalidOperationException";
import AlgebraGene from "./Gene";

export default class AlgebraGenome extends Genome<AlgebraGene>
{

	constructor(root:AlgebraGene)
	{
		super(root);
	}

	clone():AlgebraGenome
	{
		return new AlgebraGenome(this.root.clone());
	}


	serialize():string
	{
		let root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.serialize();
	}

	serializeReduced():string
	{
		let root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.asReduced().serialize();
	}

	private _hashReduced:string|undefined;
	get hashReduced():string
	{
		return this._hashReduced || (this._hashReduced = this.serializeReduced());
	}

	resetHash():void
	{
		super.resetHash();
		this._hashReduced = void 0;
	}


	calculate(values:ArrayLike<number>):number
	{
		let root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.calculate(values);
	}
}
