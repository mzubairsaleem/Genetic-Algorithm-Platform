/**
 * This is the container class for a genome.  It tracks the progress of a specific Genome.
 */
export default class Organism<TGenome extends IGenome, TFitness>
implements IOrganism<TGenome,TFitness>
{
	fitness:TFitness;

	private _hash:string;
	get hash():string
	{
		return this._hash;
	}

	constructor(
		private _genome:TGenome)
	{
		this._hash = _genome.hash;
	}

	get genome():TGenome
	{
		return this._genome;
	}
}
