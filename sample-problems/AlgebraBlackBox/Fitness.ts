import * as Procedure from "typescript-dotnet/source/System/Collections/Array/Procedure";
import {IComparable} from "typescript-dotnet/dist/commonjs/System/IComparable";

export default class AlgebraFitness implements IComparable<AlgebraFitness>
{
	private _samples:number[][];

	constructor()
	{
		this._samples = [];
		this._scores = [];
		this._count = 0;
	}

	private _count:number;
	get count():number
	{
		return this._count;
	}

	private _scores:number[];

	get scores():number[] {
		return this._scores.slice();
	}

	add(score:number[]):void
	{
		if(!score || !score.length) return;

		for(let i=0,len=score.length;i<len;i++) {
			let s = this._samples[i];
			if(!s) this._samples[i] = s = [];
			s.push(score[i]);
			this._scores[i] = null; // reset for lazy calculation.
		}

		this._count++;
	}

	getScore(index:number):number
	{
		var s = this._scores[index];
		if(!s && s!==0) this._scores[index] = s = Procedure.average(this._samples[index]);
		return s;
	}

	get hasConverged():boolean
	{
		if(this._count<10) return false;
		let len = this._scores.length;
		if(!len) return false;

		for(let i=0;i<len;i++) {
			if(this.getScore(i)!=1) return false;
		}
		return true;
	}

	compareTo(other:AlgebraFitness):number
	{
		for(let i = 0, len = this._scores.length; i<len; i++)
		{

			var a = this._scores[i], b = other.getScore(i);
			if(a<b || isNaN(a) && !isNaN(b)) return -1;

			if(a>b || !isNaN(a) && isNaN(b)) return +1;

			a = this._samples.length;
			b = other.count;
			if(a<b) return -1;

			if(a>b) return +1;

		}

		return 0;
	}
}