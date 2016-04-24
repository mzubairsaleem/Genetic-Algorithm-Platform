/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="../node_modules/typescript-dotnet/source/System/Serialization/ISerializable.d.ts"/>
///<reference path="../node_modules/typescript-dotnet/source/System/ICloneable.d.ts"/>

interface IGene
extends ISerializable, ICollection<IGene>, ICloneable<IGene>
{
	children:IGene[];
	descendants:IEnumerable<IGene>;

	clone():IGene;

	findParent(child:IGene):IGene;
}