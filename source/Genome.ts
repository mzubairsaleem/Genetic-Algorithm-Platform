/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Dictionaries/IDictionary.d.ts"/>
///<reference path="IGenome.d.ts"/>

import GeneBase from "./GeneBase";
abstract class Genome implements IGenome, ICloneable<Genome>
{

	private _root:GeneBase;
	get root():GeneBase
	{
		return this._root;
	}

	set root(value:GeneBase)
	{
		this._hash = null;
		this._root = value;
	}

	get genes():GeneBase[]
	{
		var root = this._root;
		if(!root) return [];

		return root.descendants.copyTo([root],1);
	}

	abstract compareTo(other:Genome):number;

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

	abstract clone():Genome;
}

export default Genome;
