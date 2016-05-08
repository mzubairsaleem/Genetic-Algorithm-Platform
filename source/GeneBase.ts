///<reference path="IGene.d.ts"/>
///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Enumeration/IEnumerable.d.ts"/>
import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";
import Lazy from "../node_modules/typescript-dotnet/source/System/Lazy";
import List from "../node_modules/typescript-dotnet/source/System/Collections/List";
import ArgumentException from "../node_modules/typescript-dotnet/source/System/Exceptions/ArgumentException";

abstract class GeneBase<T extends IGene>
extends List<T> implements IGene
{
	constructor()
	{
		super();
		this.resetToString();
	}

	protected _enumerable:Enumerable<T>;

	abstract serialize():string;

	abstract clone():GeneBase<T>;

	asEnumerable():Enumerable<T>
	{
		return this._enumerable || (this._enumerable = Enumerable.from(this));
	}

	get descendants():Enumerable<IGene>
	{
		var e:Enumerable<IGene> = this.asEnumerable();
		return e.concat(e.selectMany(s=>s.descendants));
	}

	findParent(child:T):IGene
	{
		var children = this._source;
		if(!children || !children.length) return null;
		if(children.indexOf(child)!=-1) return this;

		for(let c of children) {
			let p = c.findParent(child);
			if(p) return p;
		}

		return null;
	}

	protected _replaceInternal(target:T, replacement:T, throwIfNotFound?:boolean):boolean
	{
		var s = this._source;
		var index = this._source.indexOf(target);
		if(index == -1) {
			if(throwIfNotFound)
				throw new ArgumentException('target', "gene not found.");
			return false;
		}

		s[index] = replacement;
		return true;
	}

	replace(target:T, replacement:T, throwIfNotFound?:boolean):boolean
	{
		var m = this._replaceInternal(target, replacement, throwIfNotFound);
		if(m) this._onModified();
		return m;
	}

	_toString:Lazy<string>;
	protected resetToString():void
	{
		var ts = this._toString;
		if(ts) ts.reset();
		else this._toString = new Lazy<string>(()=>this.toStringInternal());
	}

	protected _onModified() {
		//super._onModified();
		this.resetToString();
	}

	protected abstract toStringInternal():string;

	toString()
	{
		return this._toString.value;
	}
}

export default GeneBase;