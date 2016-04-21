/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

interface IProblem<TGenome extends IGenome, TFitness>
{
	// Due to the complexity of potential fitness values, this provides a single place to rank a population.
	rank(
		population:IEnumerableOrArray<IOrganism<TGenome,TFitness>>):IEnumerable<IOrganism<TGenome,TFitness>>;

	// Some outlying survivors may be tied in their fitness and there needs to be a way to retain them without a hard trim.
	rankAndReduce(
		population:IEnumerableOrArray<IOrganism<TGenome,TFitness>>,
		targetMaxPopulation:number):IEnumerable<IOrganism<TGenome,TFitness>>;

	test(p:IPopulation<TGenome,TFitness>, count?:number):void;

	getGenomeFactory():IGenomeFactory<TGenome,TFitness>;
}