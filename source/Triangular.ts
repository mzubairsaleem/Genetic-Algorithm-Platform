/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {Enumerable} from "typescript-dotnet/source/System.Linq/Linq";
import {IEnumerableOrArray} from "typescript-dotnet/source/System/Collections/IEnumerableOrArray";

export function forward(n:number):number
{
	return n*(n + 1)/2;
}

export function reverse(n:number):number
{
	return (Math.sqrt(8*n + 1) - 1)/2 | 0;
}

export module disperse
{
	/**
	 * Increases the number an element based on it's index.
	 * @param source
	 * @returns {Enumerable<T>}
	 */
	export function increasing<T>(source:IEnumerableOrArray<T>):Enumerable<T>
	{
		return Enumerable.from(source)
			.selectMany<T>(
				(c, i)=>Enumerable.repeat(c, i + 1));
	}

	/**
	 * Increases the count of each element for each index.
	 * @param source
	 * @returns {Enumerable<T>}
	 */
	export function decreasing<T>(source:IEnumerableOrArray<T>):Enumerable<T>
	{
		var s = Enumerable.from(source).memoize();
		return Enumerable.from(source)
			.selectMany<T>(
				(c, i)=>s.take(i + 1));
	}

}
