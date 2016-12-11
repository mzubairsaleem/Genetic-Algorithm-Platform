/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
import {ISerializable} from "typescript-dotnet-umd/System/Serialization/ISerializable";
import {ICollection} from "typescript-dotnet-umd/System/Collections/ICollection";
import {ICloneable} from "typescript-dotnet-umd/System/ICloneable";
import {IEnumerable} from "typescript-dotnet-umd/System/Collections/Enumeration/IEnumerable";

interface IGene extends ISerializable, ICollection<IGene>, ICloneable<IGene>
{
	//children:IGene[]; Just use .toArray();
	descendants:IEnumerable<IGene>;
	findParent(child:IGene):IGene|null;

	replace(target:IGene, replacement:IGene):boolean;

	resetToString():void;
}