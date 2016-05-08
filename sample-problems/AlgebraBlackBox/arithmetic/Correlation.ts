/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

import {average} from "../../../node_modules/typescript-dotnet/source/System/Collections/Array/Procedure";
import {from as enumeratorFrom} from "../../../node_modules/typescript-dotnet/source/System/Collections/Enumeration/Enumerator";
import Exception from "../../../node_modules/typescript-dotnet/source/System/Exception";


export function abs(source:number[]):number[]
{
	return source.map(v=>isNaN(v) ? v : Math.abs(v));
}

export function deltas(source:number[]):number[]
{

	var previous:number = NaN;
	return source.map(v=>
	{
		if(!isNaN(v))
		{
			var p = previous;
			previous = v;
			if(!isNaN(p))
			{
				return v - p;
			}
		}
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


	var sourceEnumerator = enumeratorFrom(source);
	var targetEnumerator = enumeratorFrom(target);
	var result:number[] = [];

	while(true)
	{
		var sv:boolean = sourceEnumerator.moveNext();
		var tv:boolean = targetEnumerator.moveNext();

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
