import Environment from "../../source/Environment";
import AlgebraGenome from "./Genome";
import AlgebraGenomeFactory from "./GenomeFactory";
import AlgebraBlackBoxProblem from "./Problem";
import {Enumerable} from "typescript-dotnet-umd/System.Linq/Linq";
import {supplant} from "typescript-dotnet-umd/System/Text/Utility";


function actualFormula(a:number, b:number):number // Solve for 'c'.
{
	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2) + a) + b;
}

const VARIABLE_NAMES = Object.freeze(Enumerable("abcdefghijklmnopqrstuvwxyz").toArray());

export function convertParameterToAlphabet(source:string):string
{
	return supplant(source, VARIABLE_NAMES);
}

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
		try
		{
			super._onExecute();

			const problems = Enumerable(this._problems).memoize();
			const p = this._populations.linq
				.selectMany(s => s)
				.orderBy(g => g.hash.length)
				.groupBy(g => g.hashReduced)
				.select(g => g.first());

			const top = Enumerable
				.weave<{label:string,gene:AlgebraGenome}>(
					problems
						.select(r =>
							Enumerable(r.rank(p))
								.select(
									g =>
									{
										let red = g.root.asReduced(), suffix = "";
										if(red!=g.root)
											suffix
												= " => " + convertParameterToAlphabet(red.toString());
										let f = r.getFitnessFor(g);
										return {
											label: `(${f.count}) ${f.scores}: ${convertParameterToAlphabet(g.hash)}${suffix}`,
											gene: g
										};
									}
								)
						)
				)
				.take(this._problems.length)
				.memoize();

			const c = problems.selectMany(p => p.convergent).toArray();
			console.log("Top:", top.select(s=>s.label).toArray(), "\n");
			if(c.length) console.log("\nConvergent:", c.map(
				g=>convertParameterToAlphabet(g.hashReduced)));


			if(problems.count(p=>p.convergent.length!=0)<this._problems.length)
			{
				const n = this._populations.last!.value;
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

				this.start();
			}


		}
		catch(ex)
		{
			console.error(ex, ex.stack);
		}
	}


}

