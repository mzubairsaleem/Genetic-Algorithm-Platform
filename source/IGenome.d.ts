/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {ISerializable} from "typescript-dotnet-umd/System/Serialization/ISerializable";
import {IEquatable} from "typescript-dotnet-umd/System/IEquatable";
import {IGene} from "./IGene";
import {IEnumerable} from "typescript-dotnet-umd/System/Collections/Enumeration/IEnumerable";

export interface IGenome extends ISerializable, IEquatable<IGenome>
{
	disableVariations:boolean;
	root:IGene|undefined;
	hash:string;
	genes:IEnumerable<IGene>
}

export default IGenome;