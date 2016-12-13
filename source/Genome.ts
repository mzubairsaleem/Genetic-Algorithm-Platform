/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {Enumerable, LinqEnumerable} from "typescript-dotnet-umd/System.Linq/Linq";
import {ICloneable} from "typescript-dotnet-umd/System/ICloneable";
import {IGene} from "./IGene";
import {IGenome} from "./IGenome";

export abstract class Genome<T extends IGene>
implements IGenome, ICloneable<Genome<T>>
{

	constructor(private _root:T)
	{
		this._hash = null;
	}

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

	findParent(child:IGene):IGene|null
	{
		return this.root.findParent(child);
	}

	get genes():LinqEnumerable<IGene>
	{
		const root = this.root;

		return Enumerable
			.make<IGene>(root)
			.concat(root.descendants);
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
