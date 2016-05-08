/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="../node_modules/typescript-dotnet/source/System/Serialization/ISerializable.d.ts"/>
///<reference path="../node_modules/typescript-dotnet/source/System/IComparable.d.ts"/>
///<reference path="IGene.d.ts"/>

interface IGenome extends ISerializable, IEquatable<IGenome>
{
	root:IGene;
	hash:string;
	genes:IEnumerable<IGene>
}