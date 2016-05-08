/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Dictionaries/IDictionary.d.ts"/>
///<reference path="IGenome.d.ts"/>

import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";

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
		if(value!=this._root){
			this.resetHash();
			this._root = value;
		}
	}

	findParent(child:IGene):IGene
	{
		var root = this.root;
		return (root && child!=root)
			? root.findParent(child)
			: null;
	}

	get genes():Enumerable<IGene>
	{
		var root = this.root;
		return Enumerable
			.make<IGene>(root)
			.concat(root.descendants);
	}

	abstract serialize():string;

	private _hash:string;
	get hash():string
	{
		return this._hash || (this._hash = this.serialize());
	}

	resetHash():void {
		this._hash = null;
		if(this._root)
			this._root.resetToString();
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
