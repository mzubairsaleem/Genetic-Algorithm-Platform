interface IGenomeFactory<TGenome extends IGenome> {
	generate(inputParamCount?:number):TGenome;
	generateFrom(source?:IEnumerableOrArray<TGenome>):TGenome;
	mutate(source:TGenome, mutations?:number):TGenome;

	maxGenomeTracking:number;

	previousGenomes:string[];
	getPrevious(hash:string):TGenome;
	trimPreviousGenomes():void;
}

