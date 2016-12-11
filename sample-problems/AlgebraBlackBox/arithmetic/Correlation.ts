/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {average} from "typescript-dotnet-umd/System/Collections/Array/Procedure";
import {from as enumeratorFrom} from "typescript-dotnet-umd/System/Collections/Enumeration/Enumerator";
import Exception from "typescript-dotnet-umd/System/Exception";
import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";


//noinspection JSUnusedGlobalSymbols
export function abs(source:number[]):number[]
{
	return source.map(v=>isNaN(v) ? v : Math.abs(v));
}

//noinspection JSUnusedGlobalSymbols
export function deltas(source:number[]):number[]
{

	let previous:number = NaN;
	return source.map(v=>
	{
		if(!isNaN(v))
		{
			const p = previous;
			previous = v;
			if(!isNaN(p))
			{
				return v - p;
			}
		}
		return NaN;
	});

}

export function variance(source:number[]):number
{
	return average(source.map(s=>s*s)) - Math.pow(average(source), 2);
}

export function products(
	source:IEnumerableOrArray<number>,
	target:IEnumerableOrArray<number>):number[]
{


	const sourceEnumerator = enumeratorFrom(source);
	const targetEnumerator = enumeratorFrom(target);
	const result:number[] = [];

	while(true)
	{
		let sv:boolean = sourceEnumerator.moveNext();
		let tv:boolean = targetEnumerator.moveNext();

		if(sv!=tv)
			throw new Exception("Products: source and target enumerations have different counts.");

		if(!sv || !tv)
			break;

		result.push(sourceEnumerator.current*targetEnumerator.current);
	}

	return result;
}

export function covariance(source:number[], target:number[]):number
{
	return average(products(source, target)) - average(source)*average(target);
}


export function correlationUsing(
	covariance:number,
	sourceVariance:number,
	targetVariance:number):number
{
	return covariance/Math.sqrt(sourceVariance*targetVariance);
}

export function correlationOf(covariance:number, source:number[], target:number[]):number
{
	return correlationUsing(covariance, variance(source), variance(target));
}

export function correlation(source:number[], target:number[]):number
{
	return correlationOf(covariance(source, target), source, target);
}
