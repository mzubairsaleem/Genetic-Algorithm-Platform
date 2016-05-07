///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Enumeration/IEnumerateEach.d.ts"/>
import Set from "../node_modules/typescript-dotnet/source/System/Collections/Set";
import StringKeyDictionary from "../node_modules/typescript-dotnet/source/System/Collections/Dictionaries/StringKeyDictionary";
import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";
import ArgumentNullException from "../node_modules/typescript-dotnet/source/System/Exceptions/ArgumentNullException";
import {forEach} from "../node_modules/typescript-dotnet/source/System/Collections/Enumeration/Enumerator";

export default class Population<TGenome extends IGenome>
implements IPopulation<TGenome>, IEnumerateEach<TGenome>
{
	private _population:StringKeyDictionary<TGenome>;

	constructor(private _genomeFactory:IGenomeFactory<TGenome>)
	{
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
		useCopy?:boolean):void
	{
		forEach(useCopy ? this.toArray() : this, action);
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
			this.add(this._genomeFactory.generate());
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

	populate(count:number = 1):void
	{
		for(var i = 0; i<count; i++)
		{
			this.add();
		}
	}

	populateFrom(source:IEnumerableOrArray<TGenome>, count:number = 1)
	{
		//noinspection UnnecessaryLocalVariableJS
		var f = this._genomeFactory;
		// Then add mutations from best in source.
		for(var i = 0; i<count - 1; i++)
		{
			this.add(f.generateFrom(source));
		}
	}

	// Provide a mechanism for culling the herd without requiring IProblem to be imported.
	keepOnly(selected:IEnumerableOrArray<TGenome>):void
	{
		var hashed = new Set(Enumerable.from(selected).select(o=>o.hash));
		var p = this._population;
		p.forEach(o=>
		{
			var key = o.key;
			if(!hashed.contains(key))
				p.removeByKey(key);
			
		}, true);
	}
}
