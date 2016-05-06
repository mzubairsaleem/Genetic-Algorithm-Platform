/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

interface IPopulation<TGenome extends IGenome>
extends ICollection<TGenome>
{
	populate(count?:number):void;
	populateFrom(
		source:IEnumerableOrArray<TGenome>,
		count?:number,
		transferBest?:number):void;
}