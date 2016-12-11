/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {Type} from "typescript-dotnet-umd/System/Types";
import {Integer} from "typescript-dotnet-umd/System/Integer";
import {Set} from "typescript-dotnet-umd/System/Collections/Set";
import {ArgumentOutOfRangeException} from "typescript-dotnet-umd/System/Exceptions/ArgumentOutOfRangeException";
import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";
import {Random} from "typescript-dotnet-umd/System/Random";

export function nextRandomIntegerExcluding(
	range:number,
	excluding:number|IEnumerableOrArray<number>):number
{
	Integer.assert(range);
	if(range<0) throw new ArgumentOutOfRangeException("range", range, "Must be a number greater than zero.");

	const r:number[] = [],
	      excludeSet = new Set<number>(Type.isNumber(excluding, true) ? [excluding] : excluding);

	for(let i = 0; i<range; ++i)
	{
		if(!excludeSet.contains(i)) r.push(i);
	}

	return Random.select.one(r, true);
}

export default nextRandomIntegerExcluding;