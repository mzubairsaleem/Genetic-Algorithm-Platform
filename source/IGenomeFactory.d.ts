interface IGenomeFactory<TGenome extends IGenome> {
	generate(source?:TGenome[]):TGenome;
	mutate(source:TGenome, mutations?:number):TGenome;

	maxGenomeTracking:number;

	previousGenomes:string[];
	getPrevious(hash:string):TGenome;
	trimPreviousGenomes():void;
}

