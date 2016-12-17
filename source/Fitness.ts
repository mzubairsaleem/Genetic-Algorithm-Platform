import {IComparable} from "typescript-dotnet-umd/System/IComparable";

import * as Procedure from "typescript-dotnet-umd/System/Collections/Array/Procedure";
import {List} from "typescript-dotnet-umd/System/Collections/List";
import {Exception} from "typescript-dotnet-umd/System/Exception";

export class SingularFitness extends List<number>
{
	constructor()
	{
		super();
	}


	add(entry:number):void
	{
		this._average = null;
		super.add(entry);
	}

	private _average:number|null;
	get average():number
	{
		let v = this._average;
		if(v==null)
			this._average = v = Procedure.average(this._source);
		return v;
	}

}

export class Fitness extends List<SingularFitness> implements IComparable<Fitness>
{

	addTheseScores(scores:ArrayLike<number>)
	{
		const len = scores.length, count = this.count;
		for(let i = 0; i<len; i++)
		{
			let f:SingularFitness;
			if(i>=count)
				this.set(i, f = new SingularFitness());
			else
				f = this.get(i);
			f.add(scores[i]);
		}
	}

	addScores(...scores:number[])
	{
		this.addTheseScores(scores);
	}

	get sampleCount():number
	{
		if(!this.count) return 0;
		return this.linq.select(s=>s.count).min()!;
	}

	hasConverged(minSamples:number = 100, convergence:number = 1, tolerance:number = 0):boolean
	{
		if(minSamples>this.sampleCount) return false;

		for(let s of this._source)
		{
			const score = s.average;
			if(score>convergence)
				throw new Exception("Score has exceeded convergence value.");
			if(score<convergence - tolerance)
				return false;
		}
		return true;
	}


	compareTo(other:Fitness):number
	{
		const len = this._source.length;
		for(let i = 0; i<len; i++)
		{
			const a = this.get(i);
			const b = other.get(i);
			const aA = a.average;
			const bA = b.average;

			if(aA<bA || isNaN(aA) && !isNaN(bA)) return -1;
			if(aA>bA || !isNaN(aA) && isNaN(bA)) return +1;

			if(a.count<b.count) return -1;
			if(a.count>b.count) return +1;
		}
		return 0;
	}



	get scores():number[]
	{
		return this._source.map(s => s.average);
	}
}

export default Fitness;