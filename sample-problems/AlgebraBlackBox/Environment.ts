import Environment from "../../source/Environment";
import AlgebraGenome from "./Genome";
import AlgebraGenomeFactory from "./GenomeFactory";
import AlgebraBlackBoxProblem from "./Problem";
import {Enumerable} from "typescript-dotnet-umd/System.Linq/Linq";
import {Promise as NETPromise} from "typescript-dotnet-umd/System/Promises/Promise";

function actualFormula(a:number, b:number):number // Solve for 'c'.
{
	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2)) + b;
}

export default class AlgebraEnvironmentSample extends Environment<AlgebraGenome>
{

	constructor()
	{
		super(new AlgebraGenomeFactory());

		this._problems
			.push(new AlgebraBlackBoxProblem(actualFormula));

		// this.maxPopulations = 20;
		// this.populationSize = 100;
	}

	protected async _onAsyncExecute():NETPromise<void>
	{
		try
		{
			await super._onAsyncExecute();

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
												= " => " + g.toAlphaParameters(true);
										let f = r.getFitnessFor(g);
										return {
											label: `${g.toAlphaParameters()}${suffix}: (${f.count}) ${f.scores}`,
											gene: g
										};
									}
								)
						)
				)
				.take(this._problems.length)
				.memoize();

			const c = problems.selectMany(p => p.convergent).toArray();
			console.log("Top:", "\n\t"+top.select(s=>s.label).toArray().join("\n\t"));
			if(c.length) console.log("\nConvergent:", c.map(
				g=>g.toAlphaParameters(true)));


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

				this.start();
			}

			console.log("");


		}
		catch(ex)
		{
			console.error(ex, ex.stack);
		}
	}


}

