/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="IGenome.d.ts"/>
interface IOrganism<TGenome extends IGenome, TFitness>
{
	/**
	 * Uniquely identifies this organism in relation to it's peers.
	 */
	hash:string;

	/**
	 * The serializable genetic code that makes up the organism.
	 */
	genome:IGenome;

	/**
	 * Fitness could be anything.  A number, a collection of numbers, a map of numbers. It's up to the developer.
	 */
	fitness:TFitness;
}