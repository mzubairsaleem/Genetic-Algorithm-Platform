/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {average} from "typescript-dotnet-umd/System/Collections/Array/Procedure";
import {from as enumeratorFrom} from "typescript-dotnet-umd/System/Collections/Enumeration/Enumerator";
import Exception from "typescript-dotnet-umd/System/Exception";
import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";
import {SelectorWithIndex} from "typescript-dotnet-umd/System/FunctionTypes";

function map(source:ArrayLike<number>, selector:SelectorWithIndex<number,number>):Float64Array
{
	const len = source.length;
	const result = new Float64Array(len);
	for(let i=0;i<len;i++)
	{
		result[i] = selector(source[i],i);
	}
	return result;
}

//noinspection JSUnusedGlobalSymbols
export function abs(source:ArrayLike<number>):Float64Array
{
	return map(source,v => isNaN(v) ? v : Math.abs(v));
}

//noinspection JSUnusedGlobalSymbols
export function deltas(source:ArrayLike<number>):Float64Array
{

	let previous:number = NaN;
	return map(source, v =>
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

export function variance(source:ArrayLike<number>):number
{
	const len = source.length;
	const v = new Float64Array(source.length), v2 = new Float64Array(source.length);
	for(let i = 0; i<len; i++)
	{
		let s = source[i];
		v[i] = s;
		v2[i] = s*s;
	}
	return average(v2) - Math.pow(average(v), 2);
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

export function covariance(source:ArrayLike<number>, target:ArrayLike<number>):number
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

export function correlationOf(
	covariance:number, source:ArrayLike<number>,
	target:ArrayLike<number>):number
{
	return correlationUsing(covariance, variance(source), variance(target));
}

export function correlation(source:ArrayLike<number>, target:ArrayLike<number>):number
{
	return correlationOf(covariance(source, target), source, target);
}
