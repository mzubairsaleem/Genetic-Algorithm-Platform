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
		super._onExecute();
		console.log("Generation:", this._generations);

		var problems = Enumerable.from(this._problems).memoize();
		var p = Enumerable.from(this._populations).selectMany(s=>s);
		var top = Enumerable
			.weave<{label:string,gene:AlgebraGenome}>(
				problems
					.select(r=>
						Enumerable.from(r.rank(p))
							.select(
								g=>
								{
									let red = g.root.asReduced(), suffix = "";
									if(red!=g.root) suffix = " => " + supplant(red.toString(), VARIABLE_NAMES);
									return {
										label: r.getFitnessFor(g).score + ": " + supplant(g.hash, VARIABLE_NAMES) + suffix,
										gene: g
									};
								}
							)
					)
			)
			.take(this._problems.length)
			.memoize();

		var n = this._populations.last.value;
		n.importEntries(top
			.select(g=>g.gene)
			.where(g=>g.root.isReducible() && g.root.asReduced()!=g.root)
			.select(g=>
			{
				let n = g.clone();
				n.root = g.root.asReduced();
				return n;
			}));

		console.log("Population Size:", n.count);

		var c = problems.selectMany(p=>p.convergent).count();
		if(c) console.log("Convergent:", c);
		console.log("Top:", top.select(s=>s.label).toArray(), "\n");
	}


}

