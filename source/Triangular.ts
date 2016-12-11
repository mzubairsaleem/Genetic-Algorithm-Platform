/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {Enumerable, LinqEnumerable} from "typescript-dotnet-umd/System.Linq/Linq";
import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";

//noinspection JSUnusedGlobalSymbols
export function forward(n:number):number
{
	return n*(n + 1)/2;
}

//noinspection JSUnusedGlobalSymbols
export function reverse(n:number):number
{
	return (Math.sqrt(8*n + 1) - 1)/2 | 0;
}

export module disperse
{
	//noinspection JSUnusedGlobalSymbols
	/**
	 * Increases the number an element based on it's index.
	 * @param source
	 * @returns {Enumerable<T>}
	 */
	export function increasing<T>(source:IEnumerableOrArray<T>):LinqEnumerable<T>
	{
		return Enumerable(source)
			.selectMany<T>(
				(c, i)=>Enumerable.repeat(c, i + 1));
	}

	/**
	 * Increases the count of each element for each index.
	 * @param source
	 * @returns {Enumerable<T>}
	 */
	export function decreasing<T>(source:IEnumerableOrArray<T>):LinqEnumerable<T>
	{
		const s = Enumerable(source).memoize();
		return Enumerable(source)
			.selectMany<T>(
				(c, i)=>s.take(i + 1));
	}

}
