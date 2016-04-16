/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="IEnvironment.d.ts"/>
///<reference path="IGenome.d.ts"/>
///<reference path="../node_modules/typescript-dotnet/source/System/Collections/IReadOnlyCollection.d.ts"/>

interface IOrganism<TGenome extends IGenome, T extends IProblem<TGenome>>
{
	genome:IGenome;

	process(problem:T):void;

	results:IReadOnlyCollection<number>;
}