///<reference path="../node_modules/typescript-dotnet/source/System/Collections/Enumeration/IEnumerateEach.d.ts"/>
import Set from "../node_modules/typescript-dotnet/source/System/Collections/Set";
import StringKeyDictionary from "../node_modules/typescript-dotnet/source/System/Collections/Dictionaries/StringKeyDictionary";
import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";
import ArgumentNullException from "../node_modules/typescript-dotnet/source/System/Exceptions/ArgumentNullException";
import Genome from "./Genome";
import Organism from "./Organism";
import {forEach} from "../node_modules/typescript-dotnet/source/System/Collections/Enumeration/Enumerator";

export default class Population<TGenome extends Genome, TFitness>
implements IPopulation<TGenome,TFitness>, ICollection<Organism<TGenome,TFitness>>, IEnumerateEach<Organism<TGenome,TFitness>>
{
	private _population:StringKeyDictionary<Organism<TGenome,TFitness>>;

	constructor(private _genomeFactory:IGenomeFactory<TGenome,TFitness>)
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

	remove(item:Organism<TGenome, TFitness>):number
	{
		var p = this._population;
		return item && p.removeByKey(item.hash) ? 1 : 0;
	}

	clear():number
	{
		return this._population.clear();
	}

	contains(item:Organism<TGenome, TFitness>):boolean
	{
		var p = this._population;
		return !!item && p.containsKey(item.hash);
	}

	copyTo(array:Organism<TGenome, TFitness>[], index?:number):Organism<TGenome, TFitness>[]
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

	toArray():Organism<TGenome, TFitness>[]
	{
		return this.copyTo([]);
	}

	forEach(
		action:Predicate<Organism<TGenome, TFitness>>|Action<Organism<TGenome, TFitness>>,
		useCopy?:boolean):void
	{
		return forEach(useCopy ? this.toArray() : this, action);
	}

	getEnumerator():IEnumerator<Organism<TGenome, TFitness>>
	{
		return Enumerable
			.from(this._population)
			.select(o=>o.value)
			.getEnumerator();
	}

	add(potential?:Organism<TGenome,TFitness>):void
	{
		if(!potential)
		{
			// Be sure to add randomness in...
			this.addGenome(this._genomeFactory.generate());
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

	addThese(organisms:IEnumerableOrArray<Organism<TGenome,TFitness>>):void
	{
		Enumerable
			.from(organisms)
			.forEach(o=>this.add(o));
	}

	addGenome(potential:TGenome):void
	{
		var ts:string, p = this._population;
		if(potential && !p.containsKey(ts = potential.hash))
		{
			p.addByKeyValue(ts, new Organism<TGenome,TFitness>(potential));
		}
	}

	populate(count:number = 1):void
	{
		for(var i = 0; i<count; i++)
		{
			this.add();
		}
	}

	populateFrom(source:IEnumerableOrArray<Organism<TGenome,TFitness>>, count:number = 1)
	{
		//noinspection UnnecessaryLocalVariableJS
		var f = this._genomeFactory;
		// Then add mutations from best in source.
		for(var i = 0; i<count - 1; i++)
		{
			this.addGenome(f.generateFrom(source));
		}
	}

	// Provide a mechanism for culling the herd without requiring IProblem to be imported.
	keepOnly(selected:IEnumerableOrArray<Organism<TGenome,TFitness>>):void
	{
		var hashed = new Set(Enumerable.from(selected).select(o=>o.hash));
		var p = this._population;
		p.toArray().forEach(o=>
		{
			var key = o.key;
			if(!hashed.contains(key))
			{
				p.removeByKey(key);
			}
		});
	}
}
