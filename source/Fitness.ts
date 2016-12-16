import {IComparable} from "typescript-dotnet-umd/System/IComparable";

import * as Procedure from "typescript-dotnet-umd/System/Collections/Array/Procedure";
import {List} from "typescript-dotnet-umd/System/Collections/List";
import {Exception} from "typescript-dotnet-umd/System/Exception";

export const DefaultK:number = 1; // Steepness.

/**
 * The default adjuster uses a logistic function to adjust the relative values based on the number of samples.
 * @param e
 * @returns {number}
 * @constructor
 */
export function LogisticAdjuster(e:SingularFitness)
{
	return (e.average/(1 + Math.exp(-DefaultK*(e.count))) - 0.5)*2;
}

export function NoAdjuster(e:SingularFitness)
{
	return e.average;
}

export class SingularFitness extends List<number>
{
	constructor(private _adjuster:(n:SingularFitness) => number = NoAdjuster)
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

	private _adjusted:number|null;
	get adjusted():number
	{
		let v = this._adjusted;
		if(v==null)
			this._adjusted = v = this._adjuster(this);
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
		const len = this.count;
		if(len<minSamples || len==0) return false;

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
			const a = this.get(i).adjusted;
			const b = other.get(i).adjusted;

			if(a<b || isNaN(a) && !isNaN(b)) return -1;
			if(a>b || !isNaN(a) && isNaN(b)) return +1;
		}
		return 0;
	}


	get scores():number[]
	{
		return this._source.map(s => s.average);
	}
}

export default Fitness;