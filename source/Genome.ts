/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Dictionaries/IDictionary.d.ts"/>
///<reference path="IGenome.d.ts"/>

abstract class Genome<T extends IGene>
implements IGenome, ICloneable<Genome<T>>
{

	constructor(private _root?:T) {}

	get root():T
	{
		return this._root;
	}

	set root(value:T)
	{
		this._hash = null;
		this._root = value;
	}

	findParent(child:IGene):IGene
	{
		var root = this.root;
		return (root && child!=root)
			? root.findParent(child)
			: null;
	}

	abstract serialize():string;

	private _hash:string;
	get hash():string
	{
		return this._hash || (this._hash = this.serialize());
	}

	toString():string
	{
		return this.hash;
	}

	abstract clone():Genome<T>;

	equals(other:Genome<T>):boolean
	{
		return this==other || this.root==other.root || this.hash===other.hash;
	}
}

export default Genome;
