interface IGenomeFactory<TGenome extends IGenome> {
	generate(source?:IEnumerableOrArray<TGenome>):TGenome;
	mutate(source:TGenome, mutations?:number):TGenome;

	maxGenomeTracking:number;
	trimPreviousGenomes():void;
}

