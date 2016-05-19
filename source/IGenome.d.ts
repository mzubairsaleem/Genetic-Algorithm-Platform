/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {ISerializable} from "typescript-dotnet/source/System/Serialization/ISerializable";
import {IEquatable} from "typescript-dotnet/source/System/IEquatable";
import {IGene} from "./IGene";
import {IEnumerable} from "typescript-dotnet/source/System/Collections/Enumeration/IEnumerable";

export interface IGenome extends ISerializable, IEquatable<IGenome>
{
	root:IGene;
	hash:string;
	genes:IEnumerable<IGene>
}

export default IGenome;