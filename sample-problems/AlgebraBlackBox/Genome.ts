import Genome from "../../source/Genome";
import InvalidOperationException from "typescript-dotnet-umd/System/Exceptions/InvalidOperationException";
import AlgebraGene, {toAlphaParameters} from "./Gene";

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

	toAlphaParameters(reduced?:boolean):string
	{
		if(reduced)
			return this._alphaParameterHashReduced || (this._alphaParameterHashReduced
					= toAlphaParameters(this.serializeReduced()));

		return this._alphaParameterHash || (this._alphaParameterHash
				= toAlphaParameters(this.hash));
	}

	private _alphaParameterHashReduced:string|undefined;
	private _alphaParameterHash:string|undefined;
	private _hashReduced:string|undefined;
	get hashReduced():string
	{
		return this._hashReduced || (this._hashReduced = this.serializeReduced());
	}

	resetHash():void
	{
		super.resetHash();
		this._alphaParameterHashReduced =
			this._alphaParameterHash =
				this._hashReduced = void 0;
	}

	calculate(values:ArrayLike<number>):number
	{
		let root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot calculate a gene with no root.");
		return root.calculate(values);
	}

	toEntity():string
	{
		let root = this.root;
		if(!root)
			throw new InvalidOperationException("Cannot get entity with no root.");
		return root.toEntity();
	}

}
