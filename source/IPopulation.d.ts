/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="IOrganism.d.ts"/>
interface IPopulation<TOrganism extends IOrganism<IGenome,any>>
extends ICollection<TOrganism>
{
	populate(count?:number):void;
	populateFrom(
		source:IEnumerableOrArray<TOrganism>,
		count?:number,
		transferBest?:number):void;
}