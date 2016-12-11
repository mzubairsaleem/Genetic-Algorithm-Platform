import * as Procedure from "typescript-dotnet-umd/System/Collections/Array/Procedure";
import {IComparable} from "typescript-dotnet-umd/System/IComparable";

export default class Fitness implements IComparable<Fitness>
{
	private _scoreCard:number[][];

	constructor()
	{
		this._scoreCard = [];
		this._scores = [];
		this._count = 0;
	}

	private _count:number;
	get count():number
	{
		return this._count;
	}

	private _scores:Array<number|null>;

	get scores():number[] {
		return this._scores.map((v,i)=>this.getScore(i));
	}

	add(score:number[]):void
	{
		if(!score || !score.length) return;

		for(let i=0,len=score.length;i<len;i++) {
			let s = this._scoreCard[i];
			if(!s) this._scoreCard[i] = s = [];
			s.push(score[i]);
			this._scores[i] = null; // reset for lazy calculation.
		}

		this._count++;
	}

	getScore(index:number):number
	{
		let s = this._scores[index];
		if(!s && s!==0) this._scores[index] = s = Procedure.average(this._scoreCard[index]);
		return s;
	}

	hasConverged(minSamples:number=100):boolean
	{
		if(this._count<minSamples) return false;
		let len = this._scores.length;
		if(!len) return false;

		for(let i=0;i<len;i++) {
			if(this.getScore(i)!=1) return false;
		}
		return true;
	}

	compareTo(other:Fitness):number
	{
		for(let i = 0, len = this._scores.length; i<len; i++)
		{

			let a = this._scores[i], b = other.getScore(i);
			if(a<b || isNaN(<any>a) && !isNaN(b)) return -1;

			if(a>b || !isNaN(<any>a) && isNaN(b)) return +1;

		}

		let a = this._count, b = other.count;
		if(a<b) return -1;
		if(a>b) return +1;


		return 0;
	}
}