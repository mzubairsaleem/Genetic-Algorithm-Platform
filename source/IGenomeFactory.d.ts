/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */


import {IGenome} from "./IGenome";

export interface IGenomeFactory<TGenome extends IGenome>
{
	generate(source?:TGenome[]):TGenome;
	mutate(source:TGenome, mutations?:number):TGenome;

	maxGenomeTracking:number;

	previousGenomes:string[];
	getPrevious(hash:string):TGenome;
	trimPreviousGenomes():void;
}

export default IGenomeFactory;