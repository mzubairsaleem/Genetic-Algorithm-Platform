interface IGenomeFactory<TGenome extends IGenome,TFitness> {
	generate():TGenome;
	generateFrom(source:IEnumerableOrArray<Organism<TGenome,TFitness>>):TGenome;
	mutate(source:TGenome, mutations?:number):TGenome;

	maxGenomeTracking:number;
	inputParamCount:number;
	trimPreviousGenomes():void;
}

