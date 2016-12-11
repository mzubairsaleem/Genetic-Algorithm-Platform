/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

import {Set} from "typescript-dotnet-umd/System/Collections/Set";
import {StringKeyDictionary} from "typescript-dotnet-umd/System/Collections/Dictionaries/StringKeyDictionary";
import {Enumerable} from "typescript-dotnet-umd/System.Linq/Linq";
import {ArgumentNullException} from "typescript-dotnet-umd/System/Exceptions/ArgumentNullException";
import {forEach} from "typescript-dotnet-umd/System/Collections/Enumeration/Enumerator";
import {IEnumerateEach} from "typescript-dotnet-umd/System/Collections/Enumeration/IEnumerateEach";
import {
	Predicate, Action, PredicateWithIndex,
	ActionWithIndex
} from "typescript-dotnet-umd/System/FunctionTypes";
import {IEnumerator} from "typescript-dotnet-umd/System/Collections/Enumeration/IEnumerator";
import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";
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
		const p = this._population;
		return item && p.removeByKey(item.hash) ? 1 : 0;
	}

	clear():number
	{
		return this._population.clear();
	}

	contains(item:TGenome):boolean
	{
		const p = this._population;
		return !!item && p.containsKey(item.hash);
	}

	copyTo(array:TGenome[], index?:number):TGenome[]
	{
		if(!array) throw new ArgumentNullException('array');

		// This is a generic implementation that will work for all derived classes.
		// It can be overridden and optimized.
		const e = this._population.getEnumerator();
		while(e.moveNext()) // Disposes when finished.
		{
			array[index++] = e.current!.value;
		}
		return array;
	}

	toArray():TGenome[]
	{
		return this.copyTo([]);
	}

	forEach(action:Action<TGenome>, useCopy?:boolean):number;
	forEach(action:Predicate<TGenome>, useCopy?:boolean):number;
	forEach(action:ActionWithIndex<TGenome>, useCopy?:boolean):number;
	forEach(action:PredicateWithIndex<TGenome>, useCopy?:boolean):number;
	forEach(
		action:ActionWithIndex<TGenome>|PredicateWithIndex<TGenome>,
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
			const n = this._genomeFactory.generate();
			if(n) this.add(n);
		}
		else
		{
			let ts:string;
			const p = this._population;
			if(potential && !p.containsKey(ts = potential.hash))
			{
				p.addByKeyValue(ts, potential);
			}
		}
	}

	importEntries(genomes:IEnumerableOrArray<TGenome>):number
	{
		let imported = 0;
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
			const n = f.generate(rankedGenomes);
			if(n) this.add(n);
		}
	}

	// Provide a mechanism for culling the herd without requiring IProblem to be imported.
	keepOnly(selected:IEnumerableOrArray<TGenome>):void
	{
		const hashed = new Set(
			Enumerable
				.from(selected)
				.select(o => o.hash));

		const p = this._population;
		p.forEach(o=>
		{
			const key = o.key;
			if(!hashed.contains(key))
				p.removeByKey(key);
			
		}, true);
	}
}

export default Population;
