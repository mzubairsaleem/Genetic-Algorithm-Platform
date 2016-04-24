/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

interface IProblem<TGenome extends IGenome, TFitness>
{
	getFitnessFor(genome:TGenome, createIfMissing?:boolean):TFitness;
	
	// Due to the complexity of potential fitness values, this provides a single place to rank a population.
	rank(
		population:IEnumerableOrArray<TGenome>):IEnumerable<TGenome>;

	// Some outlying survivors may be tied in their fitness and there needs to be a way to retain them without a hard trim.
	rankAndReduce(
		population:IEnumerableOrArray<TGenome>,
		targetMaxPopulation:number):IEnumerable<TGenome>;

	test(p:IPopulation<TGenome>, count?:number):void;
}