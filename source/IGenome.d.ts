/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {ISerializable} from "typescript-dotnet-umd/System/Serialization/ISerializable";
import {IEquatable} from "typescript-dotnet-umd/System/IEquatable";
import {IGene} from "./IGene";
import {IEnumerable} from "typescript-dotnet-umd/System/Collections/Enumeration/IEnumerable";

export interface IGenome<T extends IGene<T>> extends ISerializable, IEquatable<IGenome<T>>
{
	// This allows for variation testing without constantly overloading.
	variationCountdown:number;
	root:T|undefined;
	hash:string;
	genes:IEnumerable<T>
}

export default IGenome;