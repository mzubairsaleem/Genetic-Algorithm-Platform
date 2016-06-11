/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {ICollection} from "typescript-dotnet-umd/System/Collections/ICollection";
import {IGenome} from "./IGenome";

export interface IPopulation<TGenome extends IGenome>
extends ICollection<TGenome>
{
	populate(
		count:number,
		source?:TGenome[]):void;
}

export default ICollection;