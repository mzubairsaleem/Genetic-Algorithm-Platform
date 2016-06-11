/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";
import {IGenome} from "./IGenome";
import {IPopulation} from "./IPopulation";

export interface IEnvironment<TGenome extends IGenome>
{
	/**
	 * Initiates a cycle of testing with the current populations and problems.
	 */
	test(count?:number):void;

	/**
	 * Spawns a new population. Optionally does so using the source provided.
	 * @param populationSize
	 * @param source
	 */
	spawn(populationSize:number, source?:IEnumerableOrArray<TGenome>):IPopulation<TGenome>;
}

export default IEnvironment;