/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {Lazy} from "typescript-dotnet-umd/System/Lazy";
import {List} from "typescript-dotnet-umd/System/Collections/List";
import {ArgumentException} from "typescript-dotnet-umd/System/Exceptions/ArgumentException";
import {IEquatable} from "typescript-dotnet-umd/System/IEquatable";
import {IGene} from "./IGene";
import {ILinqEnumerable} from "typescript-dotnet-umd/System.Linq/Enumerable";

abstract class GeneBase<T extends IGene>
extends List<T> implements IGene, IEquatable<GeneBase<T>>
{
	constructor()
	{
		super();
		this.resetToString();
	}

	abstract serialize():string;

	abstract clone():GeneBase<T>;

	get descendants():ILinqEnumerable<IGene>
	{
		const e:ILinqEnumerable<IGene> = this.linq;
		return e.concat(e.selectMany(s=>s.descendants));
	}

	findParent(child:T):IGene|null
	{
		let children = this._source;
		if(!children || !children.length) return null;
		if(children.indexOf(child)!= -1) return this;

		for(let c of children)
		{
			let p = c.findParent(child);
			if(p) return p;
		}

		return null;
	}

	protected _replaceInternal(target:T, replacement:T, throwIfNotFound?:boolean):boolean
	{
		const s = this._source;
		const index = this._source.indexOf(target);
		if(index== -1)
		{
			if(throwIfNotFound)
				throw new ArgumentException('target', "gene not found.");
			return false;
		}

		s[index] = replacement;
		return true;
	}

	replace(target:T, replacement:T, throwIfNotFound?:boolean):boolean
	{
		const m = this._replaceInternal(target, replacement, throwIfNotFound);
		if(m) this._onModified();
		return m;
	}

	_toString:Lazy<string>;

	resetToString():void
	{
		const ts = this._toString;
		if(ts) ts.tryReset();
		else this._toString = new Lazy<string>(()=>this.toStringInternal(), false, true);
		
		this.forEach(c=>c.resetToString());
	}

	protected _onModified()
	{
		super._onModified();
		this.resetToString();
	}

	protected abstract toStringInternal():string;

	toString()
	{
		return this._toString.value;
	}

	equals(other:GeneBase<T>):boolean
	{
		return this===other || this.toString()==other.toString();
	}
}

export default GeneBase;