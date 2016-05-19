import * as Procedure from "typescript-dotnet/source/System/Collections/Array/Procedure";
import ReadOnlyArrayWrapper from "typescript-dotnet/source/System/Collections/Array/ReadOnlyArrayWrapper";

export default class AlgebraFitness
{
	private _samples:number[];
	private _samplesReadOnly:ReadOnlyArrayWrapper<number>;

	constructor(){
		this._samples = [];
	}

	get scores():ReadOnlyArrayWrapper<number>
	{
		return this._samplesReadOnly || (this._samplesReadOnly
				= new ReadOnlyArrayWrapper(this._samples));
	}

	get count():number
	{
		return this._samples.length;
	}

	private _score:number = NaN;
	add(score:number):void
	{
		this._samples.push(score);
		this._score = Procedure.average(this._samples);
	}

	get score():number {
		return this._score;
	}
}