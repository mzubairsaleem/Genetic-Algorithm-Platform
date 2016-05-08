import Environment from "../../source/Environment";
import AlgebraGenome from "./Genome";
import AlgebraGenomeFactory from "./GenomeFactory";
import AlgebraBlackBoxProblem from "./Problem";
import Enumerable from "../../node_modules/typescript-dotnet/source/System.Linq/Linq";
import {supplant} from "../../node_modules/typescript-dotnet/source/System/Text/Utility";


function actualFormula(a:number, b:number):number // Solve for 'c'.
{
	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
}

const VARIABLE_NAMES = Enumerable.from("abcdefghijklmnopqrstuvwxyz").toArray();

export default class AlgebraEnvironmentSample extends Environment<AlgebraGenome>
{

	constructor()
	{
		super(new AlgebraGenomeFactory());

		this._problems
			.push(new AlgebraBlackBoxProblem(actualFormula));
	}

	protected _onExecute():void
	{
		console.log("Executing...");
		super._onExecute();

		var problems = Enumerable.from(this._problems).memoize();
		var p = Enumerable.from(this._populations).selectMany(s=>s);
		var top = Enumerable
			.weave<string>(
				problems
					.select(r=>
						Enumerable.from(r.rank(p))
							.select(g=>supplant(g.hash,VARIABLE_NAMES) +": "+ r.getFitnessFor(g).score))
			)
			.take(this._problems.length).toArray();

		console.log("Top:",top,"\n");
	}

}

