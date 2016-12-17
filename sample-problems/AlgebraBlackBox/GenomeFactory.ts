import GenomeFactoryBase from "../../source/GenomeFactoryBase";
import Enumerable from "typescript-dotnet-umd/System.Linq/Linq";
import AlgebraGenome from "./Genome";
import OperatorGene from "./Genes/Operator";
import ParameterGene from "./Genes/ParameterGene";
import * as Operator from "./Operators";
import ConstantGene from "./Genes/ConstantGene";
import nextRandomIntegerExcluding from "../../source/nextRandomIntegerExcluding";
import {Random} from "typescript-dotnet-umd/System/Random";
import {Type} from "typescript-dotnet-umd/System/Types";
import AlgebraGene from "./Gene";


export default class AlgebraGenomeFactory extends GenomeFactoryBase<AlgebraGenome>
{

	//noinspection JSMethodCanBeStatic
	protected generateParamOnly(id:number):AlgebraGenome
	{
		return new AlgebraGenome(new ParameterGene(id));
	}

	//noinspection JSMethodCanBeStatic
	protected generateOperated(paramCount:number = 1):AlgebraGenome
	{
		const op = OperatorGene.getRandomOperation();
		const result = new AlgebraGenome(op);

		for(let i = 0; i<paramCount; i++)
		{
			op.add(new ParameterGene(i));
		}

		return result;
	}


	generate(source?:AlgebraGenome[]):AlgebraGenome|null
	{

		const _ = this, p = _._previousGenomes;
		let attempts:number = 0;
		let genome:AlgebraGenome|null = null;
		let hash:string|null = null;

		// Note: for now, we will only mutate by 1.

		// See if it's possible to mutate from the provided genomes.
		if(source && source.length)
		{
			// Find one that will mutate well and use it.
			for(let m = 1; m<4; m++)
			{
				let tries = 10;//200;
				do
				{
					genome = _.mutate(Random.select.one(source, true), m);
					hash = genome.hash;
					attempts++;
				}
				while(p.containsKey(hash) && --tries);

				if(tries)
					break;
				// else
				// 	genome = null; // Failed... Converged? No solutions? Saturated?
			}

		}

		if(!genome)
		{

			for(let m = 1; m<4; m++)
			{

				// Establish a maximum.
				let tries = 10, paramCount = 0;

				do {
					{ // Try a param only version first.
						genome = this.generateParamOnly(paramCount);
						hash = genome.hash;
						attempts++;
						if(!p.containsKey(hash)) break;
					}

					paramCount += 1; // Operators need at least 2 params to start.

					{ // Then try an operator based version.
						genome = this.generateOperated(paramCount + 1);
						hash = genome.hash;
						attempts++;
						if(!p.containsKey(hash)) break;
					}

					let t = Math.min(p.count*2, 100); // A local maximum.
					do {
						genome = _.mutate(p.getValueByIndex(Random.integer(p.count)), m);
						hash = genome.hash;
						attempts++;
					}
					while(p.containsKey(hash) && --t);

					// t==0 means nothing found :(
					if(t) break;
				}
				while(--tries);

				if(tries)
					break;
				// else
				// 	genome = null; // Failed... Converged? No solutions? Saturated?
			}

		}

		//console.log("Generate attempts:",attempts);
		if(hash)
		{
			if(p.containsKey(hash))
				return p.getAssuredValue(hash);

			if(genome)
				p.addByKeyValue(hash, genome.setAsReadOnly());
		}


		// if(!genome)
		// 	throw "Failed... Converged? No solutions? Saturated?";

		return genome;

	}

	generateVariations(source:AlgebraGenome):AlgebraGenome[]
	{
		const result:AlgebraGenome[] = [];
		let sourceGenes = <AlgebraGene[]>source.genes.toArray();
		let count = sourceGenes.length;
		for(let i = 0; i<count; i++)
		{
			const gene = sourceGenes[i];
			const isRoot = gene==source.root;

			const applyClone = (
				handler:(
					gene:AlgebraGene,
					newGenome:AlgebraGenome) => boolean|void) =>
			{
				const newGenome = source.clone();
				if(handler(<AlgebraGene>newGenome.genes.elementAt(i), newGenome)!==false)
					result.push(newGenome.setAsReadOnly());
			};

			const absMultiple = Math.abs(gene.multiple);
			if(absMultiple>1)
			{
				applyClone(g =>
				{
					g.multiple -= g.multiple/absMultiple;
				});
			}

			const parentOp = Type.as(source.findParent(gene)!, OperatorGene);
			if(parentOp)
			{
				if(parentOp.count>1)
				{
					applyClone((g, newGenome) =>
					{
						let parentOp = <OperatorGene>newGenome.findParent(g);
						parentOp.remove(g);

						// Reduce to avoid NaN.
						if(parentOp.count==1 && Operator.Available.Operators.indexOf(parentOp.operator)!= -1)
						{
							if(parentOp)
							{
								let grandParent = <OperatorGene>newGenome.findParent(parentOp);
								if(grandParent)
								{
									let grandChild = parentOp.linq.single();
									grandChild.multiple *= parentOp.multiple;
									parentOp.remove(grandChild);
									grandParent.replace(parentOp, grandChild);
								}
							}
						}
					});
				}
			}

			if(gene instanceof OperatorGene && gene.count==1)
			{
				applyClone((ng, newGenome) =>
				{

					const child = ng.get(0);
					const parentOp = (<OperatorGene>newGenome.findParent(ng));

					if(isRoot)
					{
						// If the root operator is a function, swap it's contents for the root.
						newGenome.root = child;
					}
					else
					{

						parentOp!.modifyChildren(p =>
						{
							let pGenes = p.toArray();
							p.clear();
							for(let g of pGenes)
							{
								p.add(g==ng ? child! : g);
							}
							return true;
						});
					}

				});

				if(Operator.Available.Functions.indexOf(gene.operator))
				{
					applyClone((g:OperatorGene) =>
					{

						g.operator = "+";

					});
				}


			}
		}

		if(source.root instanceof OperatorGene && Operator.Available.Functions.indexOf(source.root.operator)!= -1)
		{
			// Try it without a function!

			const newGenome = source.clone();
			const first = newGenome.root.get(0);
			newGenome.root.remove(first);
			newGenome.root = first;
			result.push(newGenome);
		}
		else
		{
			// Try it with a function!

			for(let op of Operator.Available.Functions)
			{
				const newGenome = source.clone();
				const newFn = new OperatorGene(op);
				newFn.add(newGenome.root);
				newGenome.root = newFn;
				result.push(newGenome);
			}
		}

		const p = this._previousGenomes;
		return result
		//.filter(genome => !p.containsKey(genome.hash))
			.map(genome =>
			{
				genome = p.getValue(genome.hash) || genome;
				genome = genome.asReduced();
				genome = p.getValue(genome.hash) || genome;
				return genome;
			})
			//.filter(genome => !p.containsKey(genome.hash))
			;
	}

	mutate(source:AlgebraGenome, mutations:number = 1):AlgebraGenome
	{
		const inputParamCount = source.genes.ofType(ParameterGene).count();

		/* Possible mutations:
		 * 1) Adding a parameter node to an operation.
		 * 2) Apply a function to node.
		 * 3) Adding an operator and a parameter node.
		 * 4) Removing a node from an operation.
		 * 5) Removing an operation.
		 * 6) Removing a function. */

		const newGenome = source.clone();

		for(let i = 0; i<mutations; i++)
		{
			// First randomly select the gene to mutate.
			const genes = newGenome.genes.toArray();
			const gene = Random.select.one(genes, true);
			const isRoot = gene==newGenome.root;
			let parent = newGenome.findParent(gene)!;
			let parentOp = Type.as(parent, OperatorGene);

			let invalidOptions:number[]|null = [];
			const shouldNotRemove = () => isRoot || parent==null || parentOp==null;
			let doNotRemove = gene instanceof ParameterGene && shouldNotRemove();

			let lastOption = -1;

			while(invalidOptions!=null)
			{

				if(parent!=null && !parent.contains(gene))
					throw "Parent changed?";

				if(gene instanceof ConstantGene)
				{
					const cg = gene;
					switch(lastOption = nextRandomIntegerExcluding(2, invalidOptions))
					{
						// Simply alter the sign
						case 0:
							const abs = Math.abs(cg.multiple);

							if(abs>1 && Random.next(2)==0)
							{
								if(abs!=Math.floor(abs) || Random.next(2)==0)
									cg.multiple /= abs;
								else
									cg.multiple -= (cg.multiple/abs);
							}
							else
								cg.multiple *= -1;

							invalidOptions = null;
							break;



						// Remove it!
						default:

							if(parentOp!=null)
							{
								parentOp.remove(gene);
								invalidOptions = null;
							}
							break;

					}
				}

				else if(gene instanceof ParameterGene)
				{
					const pg = gene;
					switch(lastOption = nextRandomIntegerExcluding(doNotRemove
						? 4
						: 6, invalidOptions))
					{
						// Simply alter the sign
						case 0:
						{
							let abs = Math.abs(pg.multiple);

							if(abs>1 && Random.next(2)==0)
								pg.multiple /= abs;
							else
								pg.multiple *= -1;

							invalidOptions = null;
							break;
						}

						// Simply change parameters
						case 1:
							let nextParameter = nextRandomIntegerExcluding(inputParamCount + 1, pg.id);

							let newPG = new ParameterGene(nextParameter);
							if(isRoot)
								newGenome.root = newPG;
							else
								parent.replace(gene, newPG);

							invalidOptions = null;
							break;

						// Split it...
						case 2:
						{
							let newFn = OperatorGene.getRandomOperation(Operator.DIVIDE); // excluding divide

							if(isRoot)
								newGenome.root = newFn;
							else
								parent.replace(gene, newFn);

							newFn.add(gene);
							newFn.add(gene.clone());

							invalidOptions = null;
							break;
						}

						// Apply a function
						case 3:
						{
							// Reduce the pollution of functions...
							if(Random.next.inRange(0, 3)!=0)
							{
								invalidOptions.push(4);
								break;
							}

							let newFn = new OperatorGene(OperatorGene.getRandomFunctionOperator());

							if(isRoot)
								newGenome.root = newFn;
							else
								parent.replace(gene, newFn);

							newFn.add(gene);
							invalidOptions = null;
							break;
						}

						// Remove it!
						default:
							if(!parentOp)
								throw "Missing parent operator";

							if(parentOp.count<3)
							{
								if(parentOp.linq.all(
										o => o instanceof ParameterGene || o instanceof ConstantGene))
									doNotRemove = true;
								else
								{
									const replacement = parentOp.linq
										.where(o => o instanceof OperatorGene)
										.single();

									if(parentOp==newGenome.root)
										newGenome.root = replacement;
									else
										newGenome
											.findParent(parentOp)!
											.replace(parentOp, replacement);

								}
							}

							if(!doNotRemove)
							{
								parentOp.remove(gene);
								invalidOptions = null;

								// console.log(
								// 	convertParameterToAlphabet(source.toString()),
								// 	convertParameterToAlphabet(gene.toString()),
								// 	convertParameterToAlphabet(newGenome.toString())
								// );

							}
							break;


					}
				}
				else if(gene instanceof OperatorGene)
				{
					const isFn = Operator.Available.Functions.indexOf(gene.operator)!= -1;
					if(isFn)
					{
						if(!isRoot) // Might need to break it out.
							invalidOptions.push(3);
						invalidOptions.push(4);
					}

					switch(lastOption = nextRandomIntegerExcluding(doNotRemove
						? 6
						: 10, invalidOptions))
					{
						// Simply invert the sign
						case 0:
						{
							gene.multiple *= -1;
							invalidOptions = null;
							break;
						}

						// Simply change operations
						case 1:
						{
							let currentOperatorIndex
								    = Operator.Available.Operators.indexOf(gene.operator);
							if(currentOperatorIndex== -1)
							{
								currentOperatorIndex
									= Operator.Available.Functions.indexOf(gene.operator);
								if(currentOperatorIndex!= -1)
								{
									if(Operator.Available.Functions.length==1)
									{
										invalidOptions.push(1);
										break;
									}

									gene.operator = Operator.Available.Functions[
										nextRandomIntegerExcluding(Operator.Available.Functions.length, currentOperatorIndex)];
								}


								break;
							}

							const newOperatorIndex = nextRandomIntegerExcluding(Operator.Available.Operators.length, currentOperatorIndex);

							// Decide if we will also change the grouping.
							if(gene.count>2 && Random.next(gene.count)!=0)
							{
								let startIndex = Random.next(gene.count - 1);
								let endIndex = startIndex==0
									? Random.next.inRange(1, gene.count - 1)
									: Random.next.inRange(startIndex + 1, gene.count);

								gene.modifyChildren(v =>
								{
									const contents = Enumerable(v)
										.skip(startIndex)
										.take(endIndex - startIndex)
										.toArray();

									for(let o of contents)
									{
										v.remove(o);
									}

									const O = OperatorGene.getRandomOperation();
									O.importEntries(contents);
									v.insert(startIndex, O);

									return true;
								});
							}
							else // Grouping remains... Only operator changes.
								gene.operator = Operator.Available.Operators[newOperatorIndex];

							invalidOptions = null;
							break;
						}

						// Add random parameter.
						case 2:
						{
							if(gene.operator==Operator.SQUARE_ROOT
								// In order to avoid unnecessary reduction, avoid adding subsequent divisors.
								|| gene.operator==Operator.DIVIDE && gene.count>1)
								break;

							gene.add(new ParameterGene(Random.next(inputParamCount)));
							invalidOptions = null;
							break;
						}

						// Add random operator branch.
						case 3:
						{
							const n = new ParameterGene(Random.next(inputParamCount));

							const newOp = inputParamCount<=1
								? OperatorGene.getRandomOperation('/')
								: OperatorGene.getRandomOperation();

							if(isFn || Random.next(4)==0)
							{
								// logic states that this MUST be the root node.
								const index = Random.next(2);
								if(index) {
									newOp.add(n);
									newOp.add(newGenome.root);
								} else {
									newOp.add(newGenome.root);
									newOp.add(n);
								}

								newGenome.root = newOp;
							}
							else
							{

								newOp.add(n);

								// Useless to divide a param by itself, avoid...
								if(newOp.operator==Operator.DIVIDE)
									newOp.add(new ParameterGene(nextRandomIntegerExcluding(inputParamCount, n.id)));
								else
									newOp.add(new ParameterGene(Random.next(inputParamCount)));

								gene.add(newOp);
								invalidOptions = null;

							}

							break;
						}

						// Apply a function
						case 4:
						{
							// // Reduce the pollution of functions...
							// if(Random.next(4)!=1)
							// {
							// 	break;
							// }
							//
							// Reduce the pollution of functions...
							if(Operator.Available.Functions.indexOf(gene.operator)!= -1 && Random.next(4)!=1)
							{
								break;
							}

							const newFn = new OperatorGene(OperatorGene.getRandomFunctionOperator());

							//
							// // Reduce the pollution of functions...
							// if(newFn.operator==og.operator)
							// {
							// 	if(Random.next(7)!=1)
							// 	{
							// 		invalidOptions.push(5);
							// 		break;
							// 	}
							// }

							if(isRoot)
								newGenome.root = newFn;
							else
								parent.replace(gene, newFn);

							newFn.add(gene);
							invalidOptions = null;
							break;
						}
						case 5:
						{
							if(gene.reduce())
								invalidOptions = null;
							break;
						}
						// Remove it!
						default:
						{

							if(Operator.Available.Functions.indexOf(gene.operator)!= -1)
							{
								if(isRoot)
								{
									if(gene.count)
										newGenome.root = gene.linq.first();
									else
									{
										doNotRemove = true;
										break;
									}

								}
								else
								{

									parentOp!.modifyChildren(v =>
									{
										const index = v.indexOf(gene);
										if(index!= -1)
										{
											for(let o of gene.toArray().reverse())
											{
												v.insert(index, o);
											}
											v.remove(gene);
											invalidOptions = null;
											return true;
										}
										return false;
									});

								}
								break;
							}

							// Just like above, consider reduction instead of trimming...
							if(isRoot && gene.count>2)
							{
								gene.removeAt(Random.next(gene.count));
							}
							else if(gene.count==2
								&& gene.linq.any(o => o instanceof OperatorGene)
								&& gene.linq.any(o => o instanceof ParameterGene))
							{
								let childOpGene = gene.linq.ofType(OperatorGene).single();
								gene.remove(childOpGene);
								if(isRoot)
									newGenome.root = childOpGene;
								else
									parentOp!.replace(gene, childOpGene);
							}
							else if(shouldNotRemove() || gene.count>2)
							{
								doNotRemove = true;
								break;
							}
							else
								parentOp!.remove(gene);

							invalidOptions = null;
							break;

						}
					}
				}
			}

		}

		newGenome.resetHash();
		return newGenome.setAsReadOnly();


	}

}
