///<reference path="IGene.d.ts"/>
///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Enumeration/IEnumerable.d.ts"/>
import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";
import Lazy from "../node_modules/typescript-dotnet/source/System/Lazy";
import List from "../node_modules/typescript-dotnet/source/System/Collections/List";

abstract class GeneBase<T extends GeneBase>
extends List<T> implements IGene
{
	constructor()
	{
		super();
		this.resetToString();
	}

	protected _enumerable:Enumerable<T>;

	abstract serialize():string;

	abstract clone():GeneBase;

	get children():IGene[] {
		return this._source.slice();
	}

	asEnumerable():Enumerable<T>
	{
		return this._enumerable || (this._enumerable = Enumerable.from(this));
	}

	get descendants():Enumerable<T>
	{
		var c = this.asEnumerable;
		return c.concat(c.selectMany(s=>s.descendants));
	}

	findParent(child:T):T
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