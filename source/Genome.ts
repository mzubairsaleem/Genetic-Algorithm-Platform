/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Dictionaries/IDictionary.d.ts"/>
///<reference path="IGenome.d.ts"/>

abstract class Genome implements IGenome {

	fitness:IMap<number>;

	abstract compareTo(other:Genome):number;
}

export default Genome;
