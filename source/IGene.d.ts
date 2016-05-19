/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {ISerializable} from "typescript-dotnet/source/System/Serialization/ISerializable";
import {ICollection} from "typescript-dotnet/source/System/Collections/ICollection";
import {ICloneable} from "typescript-dotnet/source/System/ICloneable";
import {IEnumerable} from "typescript-dotnet/source/System/Collections/Enumeration/IEnumerable";

interface IGene extends ISerializable, ICollection<IGene>, ICloneable<IGene>
{
	clone():IGene;

	//children:IGene[]; Just use .toArray();
	descendants:IEnumerable<IGene>;
	findParent(child:IGene):IGene;

	replace(target:IGene, replacement:IGene):boolean;

	resetToString():void;
}