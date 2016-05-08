import Type from "../node_modules/typescript-dotnet/source/System/Types";
import Integer from "../node_modules/typescript-dotnet/source/System/Integer";
import Set from "../node_modules/typescript-dotnet/source/System/Collections/Set";
import ArgumentOutOfRangeException from "../node_modules/typescript-dotnet/source/System/Exceptions/ArgumentOutOfRangeException";

export default function nextRandomIntegerExcluding(
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
		console.log(range,excluding,r);
		throw new Error("Invalid select.");
	}

	return Integer.random.select(r);
}
