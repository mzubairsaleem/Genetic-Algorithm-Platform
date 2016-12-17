/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {Enumerable, LinqEnumerable} from "typescript-dotnet-umd/System.Linq/Linq";
import {ICloneable} from "typescript-dotnet-umd/System/ICloneable";
import {IGene} from "./IGene";
import {IGenome} from "./IGenome";

export abstract class Genome<T extends IGene<T>>
implements IGenome<T>, ICloneable<Genome<T>>
{

	constructor(private _root:T)
	{
		this._hash = null;
	}

	//noinspection JSUnusedGlobalSymbols
	variationCountdown:number = 0;

	get root():T
	{
		return this._root;
	}

	set root(value:T)
	{
		if(value!=this._root)
		{
			this.resetHash();
			this._root = value;
		}
	}

	findParent(child:T):IGene<T>|null
	{
		return this._root.findParent(child);
	}

	get genes():LinqEnumerable<T>
	{
		const root = this._root;

		return Enumerable
			.make(root)
			.concat(root.descendants);
	}

	setAsReadOnly():this {
		this.root.setAsReadOnly();
		return this;
	}

	abstract serialize():string;

	private _hash:string|null;
	get hash():string
	{
		return this._hash || (this._hash = this.serialize());
	}

	resetHash():void
	{
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
