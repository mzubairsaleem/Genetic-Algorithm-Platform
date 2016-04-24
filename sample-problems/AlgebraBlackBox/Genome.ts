import Genome from "../../source/Genome";
import NotImplementedException from "../../node_modules/typescript-dotnet/source/System/Exceptions/NotImplementedException";

export default class AlgebraGenome extends Genome {
	clone():Genome
	{
		throw new NotImplementedException();
	}

	compareTo(other:Genome):number
	{
		throw new NotImplementedException();
	}

	serialize():string
	{
		throw new NotImplementedException();
	}

	calculate(values:number[]):number {
		throw new NotImplementedException();
	}
}
