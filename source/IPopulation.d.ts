/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

interface IPopulation<TGenome extends IGenome,TFitness>
extends ICollection<IOrganism<TGenome,TFitness>>
{
	addGenome(potential:TGenome):void;

	populate(count?:number):void;
	populateFrom(
		source:IEnumerableOrArray<IOrganism<TGenome,TFitness>>,
		count?:number,
		transferBest?:number):void;
}