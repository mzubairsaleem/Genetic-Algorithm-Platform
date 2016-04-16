/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

interface IProblem<TGenome extends IGenome> {
	compare(a:TGenome,b:TGenome):boolean;

	test(p:IPopulation, count?:number):void;
}