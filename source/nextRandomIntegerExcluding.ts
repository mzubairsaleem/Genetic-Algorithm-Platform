/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {Type} from "typescript-dotnet/source/System/Types";
import {Integer} from "typescript-dotnet/source/System/Integer";
import {Set} from "typescript-dotnet/source/System/Collections/Set";
import {ArgumentOutOfRangeException} from "typescript-dotnet/source/System/Exceptions/ArgumentOutOfRangeException";
import {IEnumerableOrArray} from "../node_modules/typescript-dotnet/source/System/Collections/IEnumerableOrArray";

export function nextRandomIntegerExcluding(
	range:number,
	excluding:number|IEnumerableOrArray<number>):number
{
	Integer.assert(range);
	if(range<0) throw new ArgumentOutOfRangeException("range", range, "Must be a number greater than zero.");

	var r:number[] = [],
	    excludeSet = new Set<number>(Type.isNumber(excluding, true) ? [excluding] : excluding);

	for(let i = 0; i<range; ++i)
	{
		if(!excludeSet.contains(i)) r.push(i);
	}

	if(!r.length)
	{
		console.log(range, excluding, r);
		throw new Error("Invalid select.");
	}

	return Integer.random.select(r);
}

export default nextRandomIntegerExcluding;