/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
import {ISerializable} from "typescript-dotnet-umd/System/Serialization/ISerializable";
import {ICollection} from "typescript-dotnet-umd/System/Collections/ICollection";
import {ICloneable} from "typescript-dotnet-umd/System/ICloneable";
import {ILinqEnumerable} from "typescript-dotnet-umd/System.Linq/Enumerable";

interface IGene<T extends IGene<T>> extends ISerializable, ICollection<IGene<T>>, ICloneable<IGene<T>>
{
	//children:IGene[]; Just use .toArray();
	descendants:ILinqEnumerable<T>;
	findParent(child:T):IGene<T>|null;
	replace(target:T, replacement:T):boolean;
	resetToString():void;
	setAsReadOnly():this;
}