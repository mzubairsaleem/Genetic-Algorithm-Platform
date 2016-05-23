/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

import {Set} from "typescript-dotnet/source/System/Collections/Set";
import {StringKeyDictionary} from "typescript-dotnet/source/System/Collections/Dictionaries/StringKeyDictionary";
import {Enumerable} from "typescript-dotnet/source/System.Linq/Linq";
import {ArgumentNullException} from "typescript-dotnet/source/System/Exceptions/ArgumentNullException";
import {forEach} from "typescript-dotnet/source/System/Collections/Enumeration/Enumerator";
import {IEnumerateEach} from "typescript-dotnet/source/System/Collections/Enumeration/IEnumerateEach";
import {Predicate, Action} from "typescript-dotnet/source/System/FunctionTypes";
import {IEnumerator} from "typescript-dotnet/source/System/Collections/Enumeration/IEnumerator";
import {IEnumerableOrArray} from "typescript-dotnet/source/System/Collections/IEnumerableOrArray";
import {IGenome} from "./IGenome";
import {IPopulation} from "./IPopulation";
import {IGenomeFactory} from "./IGenomeFactory";

export class Population<TGenome extends IGenome>
implements IPopulation<TGenome>, IEnumerateEach<TGenome>
{
	private _population:StringKeyDictionary<TGenome>;

	constructor(private _genomeFactory:IGenomeFactory<TGenome>)
	{
		this._population = new StringKeyDictionary<TGenome>();
	}


	get isReadOnly():boolean
	{
		return false;
	}

	get count():number
	{
		return this._population.count;
	}

	remove(item:TGenome):number
	{
		var p = this._population;
		return item && p.removeByKey(item.hash) ? 1 : 0;
	}

	clear():number
	{
		return this._population.clear();
	}

	contains(item:TGenome):boolean
	{
		var p = this._population;
		return !!item && p.containsKey(item.hash);
	}

	copyTo(array:TGenome[], index?:number):TGenome[]
	{
		if(!array) throw new ArgumentNullException('array');

		// This is a generic implementation that will work for all derived classes.
		// It can be overridden and optimized.
		var e = this._population.getEnumerator();
		while(e.moveNext()) // Disposes when finished.
		{
			array[index++] = e.current.value;
		}
		return array;
	}

	toArray():TGenome[]
	{
		return this.copyTo([]);
	}

	forEach(
		action:Predicate<TGenome>|Action<TGenome>,
		useCopy?:boolean):number
	{
		return forEach(useCopy ? this.toArray() : this, action);
	}

	getEnumerator():IEnumerator<TGenome>
	{
		return Enumerable
			.from(this._population)
			.select(o=>o.value)
			.getEnumerator();
	}

	add(potential?:TGenome):void
	{
		if(!potential)
		{
			// Be sure to add randomness in...
			var n = this._genomeFactory.generate();
			if(n) this.add(n);
		}
		else
		{
			var ts:string, p = this._population;
			if(potential && !p.containsKey(ts = potential.hash))
			{
				p.addByKeyValue(ts, potential);
			}
		}
	}

	importEntries(genomes:IEnumerableOrArray<TGenome>):number
	{
		var imported = 0;
		forEach(genomes, o=>
		{
			this.add(o);
			imported++;
		});
		return imported;
	}

	populate(count:number, rankedGenomes?:TGenome[]):void
	{
		//noinspection UnnecessaryLocalVariableJS
		let f = this._genomeFactory;
		// Then add mutations from best in source.
		for(let i = 0; i<count; i++)
		{
			var n = f.generate(rankedGenomes);
			if(n) this.add(n);
		}
	}

	// Provide a mechanism for culling the herd without requiring IProblem to be imported.
	keepOnly(selected:IEnumerableOrArray<TGenome>):void
	{
		var hashed = new Set(
			Enumerable
				.from(selected)
				.select(o=>o.hash));

		var p = this._population;
		p.forEach(o=>
		{
			var key = o.key;
			if(!hashed.contains(key))
				p.removeByKey(key);
			
		}, true);
	}
}

export default Population;
