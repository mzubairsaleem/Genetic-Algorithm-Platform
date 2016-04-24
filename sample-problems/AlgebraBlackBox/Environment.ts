import Environment from "../../source/Environment";
import AlgebraGenome from "./Genome";
import AlgebraGenomeFactory from "./GenomeFactory";
import AlgebraBlackBoxProblem from "./Problem";

function actualFormula(a:number, b:number):number // Solve for 'c'.
{
	return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
}


export default class AlgebraEnvironmentSample extends Environment<AlgebraGenome> {

	constructor() {
		super(new AlgebraGenomeFactory());

		this._problems
			.push(new AlgebraBlackBoxProblem(actualFormula));
	}

}

