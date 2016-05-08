import GeneBase from "../../source/GeneBase";

const EMPTY:string = "";

abstract class AlgebraGene
extends GeneBase<AlgebraGene> {

	constructor(protected _multiple:number = 1)
	{
		super();
	}

	abstract clone():AlgebraGene;

	abstract asReduced():AlgebraGene;

	serialize():string {
		return this.toString();
	}

	get multiple():number
	{
		return this._multiple;
	}

	set multiple(value:number) {
		this._multiple = value;
		this._onModified();
	}

	protected get multiplePrefix():string
	{
		var m = this._multiple;
		if (m != 1)
			return m == -1 ? "-" : (m+EMPTY);

		return EMPTY;
	}

	abstract toStringContents():string;

	protected toStringInternal():string
	{
		return this.multiplePrefix
			+ this.toStringContents();
	}
	
	calculate(values:number[]):number
	{
		return this._multiple
			* this.calculateWithoutMultiple(values);
	}

	protected abstract calculateWithoutMultiple(values:number[]):number;

}

export default AlgebraGene;