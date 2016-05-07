import GenomeFactoryBase from "../../source/GenomeFactoryBase";
import Enumerable from "../../node_modules/typescript-dotnet/source/System.Linq/Linq";
import Integer from "../../node_modules/typescript-dotnet/source/System/Integer";
import AlgebraGenome from "./Genome";
import OperatorGene from "./Genes/Operator";
import ParameterGene from "./Genes/ParameterGene";
import * as Operator from "./Operators";
import ConstantGene from "./Genes/ConstantGene";
import nextRandomIntegerExcluding from "../../source/nextRandomIntegerExcluding";


export default class AlgebraGenomeFactory extends GenomeFactoryBase<AlgebraGenome>
{

	protected generateParamOnly(id:number):AlgebraGenome
	{
		var result = new AlgebraGenome();
		result.root = new ParameterGene(id);
		return result;
	}

	protected generateOperated(paramCount:number = 1):AlgebraGenome
	{
		var result = new AlgebraGenome();
		var op = OperatorGene.getRandomOperation();
		result.root = op;

		for(let i = 0; i<paramCount; i++)
		{
			op.add(new ParameterGene(i));
		}

		return result;
	}

	generate(source?:AlgebraGenome[]):AlgebraGenome
	{
		var _ = this, p = _._previousGenomes;
		var genome:AlgebraGenome;
		var hash:string;

		// Note: for now, we will only mutate by 1.

		// See if it's possible to mutate from the provided genomes.
		if(source && source.length)
		{
			// Find one that will mutate well and use it.
			let tries = 1000;
			do
			{
				genome = this.mutate(Integer.random.select(source));
				hash = genome.hash;
			}
			while(p.containsKey(hash) && --tries);

			if(!tries)
				genome = null; // Failed... Converged? No solutions? Saturated?
		}

		if(!genome)
		{

			// Establish a maximum.
			let tries = 10, paramCount = 0;
			var genome:AlgebraGenome, hash:string;

			do {
				{ // Try a param only version first.
					genome = this.generateParamOnly(paramCount);
					hash = genome.hash;
					if(!p.containsKey(hash)) break;
				}

				paramCount++;

				{ // Then try an operator based version.
					genome = this.generateOperated(paramCount);
					hash = genome.hash;
					if(!p.containsKey(hash)) break;
				}

				let t = Math.min(p.count*2, 100); // A local maximum.
				do {
					genome = _.mutate(p.getValueByIndex(Integer.random(p.count)));
					hash = genome.hash;
				}
				while(p.containsKey(hash) && --t);

				// t==0 means nothing found :(
				if(t) break;
			}
			while(--tries);

			if(!tries)
				genome = null; // Failed... Converged? No solutions? Saturated?

		}


		if(genome && hash)
			p.addByKeyValue(hash, genome);

		return genome;
	}

	mutate(source:AlgebraGenome, mutations:number = 1):AlgebraGenome
	{
		var inputParamCount = source.root.descendants.ofType(ParameterGene).count();

		/* Possible mutations:
		 * 1) Adding a parameter node to an operation.
		 * 2) Apply a function to node.
		 * 3) Adding an operator and a parameter node.
		 * 4) Removing a parameter from an operation.
		 * 5) Removing an operation.
		 * 6) Removing a function. */

		var newGenome = source.clone();

		for(var i = 0; i<mutations; i++)
		{
			// First randomly select the gene to mutate.
			let genes = newGenome.root.descendants.toArray();
			let gene = Integer.random.select(genes);
			let isRoot = gene==newGenome.root;
			let parent = newGenome.findParent(gene);
			let parentOp:OperatorGene = parent instanceof OperatorGene ? parent : null;

			let invalidOptions:number[] = [];
			let shouldNotRemove = ()=>isRoot || parent==null || parentOp==null;
			let doNotRemove = gene instanceof ParameterGene && shouldNotRemove();

			let lastOption = -1;

			while(invalidOptions!=null)
			{

				if(parent!=null && !parent.contains(gene))
					throw "Parent changed?";

				if(gene instanceof ConstantGene)
				{
					var cg = gene;
					switch(lastOption = nextRandomIntegerExcluding(2, invalidOptions))
					{
						// Simply alter the sign
						case 0:
							var abs = Math.abs(cg.multiple);

							if(abs>1 && Integer.random.next(2)==0)
							{
								if(abs!=Math.floor(abs) || Integer.random.next(2)==0)
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
					var pg = gene;
					switch(lastOption = nextRandomIntegerExcluding(doNotRemove
						? 4
						: 8, invalidOptions))
					{
						// Simply alter the sign
						case 0:
						{
							let abs = Math.abs(pg.multiple);

							if(abs>1 && Integer.random.next(2)==0)
								pg.multiple /= abs;
							else
								pg.multiple *= -1;

							invalidOptions = null;
							break;
						}

						// Simply change parameters
						case 1:
							let nextParameter = nextRandomIntegerExcluding(inputParamCount, pg.id);

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
							let newFn = OperatorGene.getRandomOperation(Operator.DIVIDE);

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
							if(Integer.random.nextInRange(0, 3)!=0)
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
							if(parentOp.count<3)
							{
								if(parentOp.asEnumerable().all(
										o => o instanceof ParameterGene || o instanceof ConstantGene))
									doNotRemove = true;
								else
								{
									var replacement = parentOp.asEnumerable().where(
										o => o instanceof OperatorGene).single();
									if(parentOp==newGenome.root)
										newGenome.root = replacement;
									else
										newGenome
											.findParent(parentOp)
											.replace(parentOp, replacement);

								}
							}

							if(!doNotRemove)
							{
								parentOp.remove(gene);
								invalidOptions = null;
							}
							break;


					}
				}
				else if(gene instanceof OperatorGene)
				{
					let og = gene;
					if(Operator.Available.Functions.indexOf(og.operator)!= -1)
					{
						invalidOptions.push(3);
						invalidOptions.push(4);
					}

					switch(lastOption = nextRandomIntegerExcluding(doNotRemove
						? 6
						: 10, invalidOptions))
					{
						// Simply invert the sign
						case 0:
							og.multiple *= -1;
							invalidOptions = null;
							break;

						// Simply change operations
						case 1:
							var currentOperatorIndex
								    = Operator.Available.Operators.indexOf(og.operator);
							if(currentOperatorIndex== -1)
							{
								currentOperatorIndex
									= Operator.Available.Functions.indexOf(og.operator);
								if(currentOperatorIndex!= -1)
								{
									if(Operator.Available.Functions.length==1)
									{
										invalidOptions.push(1);
										break;
									}

									og.operator = Operator.Available.Functions[
										nextRandomIntegerExcluding(Operator.Available.Functions.length, currentOperatorIndex)];
								}


								break;
							}

							var newOperatorIndex = nextRandomIntegerExcluding(Operator.Available.Operators.length, currentOperatorIndex);

							// Decide if we will also change the grouping.
							if(og.count>2 && Integer.random.next(og.count)!=0)
							{
								let startIndex = Integer.random.next(og.count - 1);
								let endIndex = startIndex==0
									? Integer.random.nextInRange(1, og.count - 1)
									: Integer.random.nextInRange(startIndex + 1, og.count);

								og.modifyChildren(v =>
								{
									var contents = Enumerable.from(v)
										.skip(startIndex)
										.take(endIndex - startIndex)
										.toArray();

									for(var o of contents)
									{
										v.remove(o);
									}

									var O = OperatorGene.getRandomOperation();
									O.importEntries(contents);
									v.insert(startIndex, O);

									return true;
								});
							}
							else // Grouping remains... Only operator changes.
								og.operator = Operator.Available.Operators[newOperatorIndex];

							invalidOptions = null;
							break;


						// Add random parameter.
						case 2:
							// In order to avoid unnecessary reduction, avoid adding subsequent divisors.
							if(og.operator==Operator.DIVIDE && og.count>1)
								break;

							og.add(new ParameterGene(Integer.random.next(inputParamCount)));
							invalidOptions = null;
							break;

						// Add random operator branch.
						case 3:


							var first = new ParameterGene(Integer.random.next(inputParamCount));

							var newOp = inputParamCount==1
								? OperatorGene.getRandomOperation('/')
								: OperatorGene.getRandomOperation();

							newOp.add(first);

							// Useless to divide a param by itself, avoid...
							if(newOp.operator==Operator.DIVIDE)
								newOp.add(new ParameterGene(nextRandomIntegerExcluding(inputParamCount, first.id)));
							else
								newOp.add(new ParameterGene(Integer.random.next(inputParamCount)));

							og.add(newOp);
							invalidOptions = null;
							break;
						// Apply a function
						case 4:
						{
							// Reduce the pollution of functions...
							if(Integer.random.next(4)!=1)
							{
								break;
							}

							// Reduce the pollution of functions...
							if(Operator.Available.Functions.indexOf(og.operator)!= -1 && Integer.random.next(4)!=1)
							{
								break;
							}

							var newFn = new OperatorGene(OperatorGene.getRandomFunctionOperator());


							// Reduce the pollution of functions...
							if(newFn.operator==og.operator)
							{
								if(Integer.random.next(7)!=1)
								{
									invalidOptions.push(5);
									break;
								}
							}

							if(isRoot)
								newGenome.root = newFn;
							else
								parent.replace(gene, newFn);

							newFn.add(gene);
							invalidOptions = null;
							break;
						}
						case 5:
							if(og.reduce())
								invalidOptions = null;
							break;
						// Remove it!
						default:
							if(Operator.Available.Functions.indexOf(og.operator)!= -1)
							{
								if(isRoot)
								{
									if(og.count)
										newGenome.root = og.asEnumerable().first();
									else
									{
										doNotRemove = true;
										break;
									}

								}
								else
								{
									parentOp.modifyChildren(v =>
									{
										var index = v.indexOf(gene);
										if(index!= -1)
										{
											for(let o of og.toArray().reverse())
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
							if(isRoot && og.count>2)
							{
								og.removeAt(Integer.random.next(og.count));
							}
							else if(og.count==2
								&& og.asEnumerable().any(o => o instanceof OperatorGene)
								&& og.asEnumerable().any(o => o instanceof ParameterGene))
							{
								let childOpGene = og.asEnumerable().ofType(OperatorGene).single();
								og.remove(childOpGene);
								if(isRoot)
									newGenome.root = childOpGene;
								else
									parentOp.replace(og, childOpGene);
							}
							else if(shouldNotRemove() || og.count<3)
							{
								doNotRemove = true;
								break;
							}
							else
								parentOp.remove(gene);

							invalidOptions = null;
							break;


					}
				}
			}

		}

		return newGenome;


	}

}
