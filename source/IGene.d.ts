/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="../node_modules/typescript-dotnet/source/System/Serialization/ISerializable.d.ts"/>
///<reference path="../node_modules/typescript-dotnet/source/System/ICloneable.d.ts"/>

interface IGene
extends ISerializable, ICollection<IGene>, ICloneable<IGene>
{
	clone():IGene;

	//children:IGene[]; Just use .toArray();
	descendants:IEnumerable<IGene>;
	findParent(child:IGene):IGene;

	replace(target:IGene, replacement:IGene):boolean;

	resetToString():void;
}