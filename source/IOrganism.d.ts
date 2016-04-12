/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="IProblem.d.ts"/>
///<reference path="IGenome.d.ts"/>

interface IOrganism<T extends IProblem>
{
	genome:IGenome;

	process(problem:T):void;

	results:IReadOnlyCollection<double>;
}