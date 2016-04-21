import Genome from "../source/Genome";
import GenomeFactoryBase from "../source/GenomeFactoryBase";
import Enumerable from "../node_modules/typescript-dotnet/source/System.Linq/Linq";
import Integer from "../node_modules/typescript-dotnet/source/System/Integer";
import Organism from "../source/Organism";
import Environment from "./../source/Environment";
import AlgebraGenome from "./AlgebraGenome";
import AlgebraFitness from "./AlgebraFitness";

export default class AlgebraGenomeFactory
	extends GenomeFactoryBase<AlgebraGenome,AlgebraFitness>
{

	protected generateSimple():AlgebraGenome
	{
		var result = new Genome();
		var op = new OperatorGene();
		op.Operator = OperatorGene.GetRandomOperator();
		result.Root = op;

		for(var i = 0, len = this._inputParamCount; i<len; i++)
		{
			op.Add(new ParameterGene(i));
		}

		return result;
	}

	generate():AlgebraGenome
	{
		var _ = this, p = _._previousGenomes, ep = Enumerable.from(p);
		var tries = 0;

		var genome = this.generateSimple();
		var gs:string;
		gs = genome.hash;
		while(ep.any(g => g.hash==gs) && ++tries<1000)
		{
			genome = _.mutate(p.getValueAt(Integer.random.under(p.count)));
			gs = genome.hash;
			// Is it there some weird possibility that this could get stuck?
		}

		if(tries>=1000)
			return null; // Failed... Converged? No solutions? Saturated?

		this._previousGenomes.add(genome);

		return genome;
	}

	generateFrom(source:IEnumerableOrArray<Organism<AlgebraGenome, AlgebraFitness>>):AlgebraGenome
	{
		var sourceGenomes = Enumerable
			.from(source)
			.orderBy(s => s.fitness.score)
			.select(s => s.genome).toArray();

		// Use a ranking weighted random selection.
		var selectionList:Genome[] = [];
		for(var i = 0; i<sourceGenomes.length; i++)
		{
			var g = sourceGenomes[i];
			for(var n = 0; n<i + 1; n++)
			{
				selectionList.push(g);
			}
		}

		var tries = 0;
		var genome:Genome;
		var gs:string;
		do
		{
			genome = Mutate(selectionList[Environment.Randomizer.Next(selectionList.length)]);
			gs = genome.CachedToStringReduced;
			// Is it there some weird possibility that this could get stuck?
		}
		while(_previousGenomes.Any(g => g.CachedToStringReduced==gs) && ++tries<1000);

		if(tries>=1000)
			return null; // Failed... Converged? No solutions? Saturated?

		_previousGenomes.Add(genome);

		return genome;
	}

	mutate(source:AlgebraGenome, mutations:number = 1):AlgebraGenome
	{
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
			var genes = newGenome.Genes.ToArray();
			var g = Environment.Randomizer.Next(genes.Length);
			var gene = genes[g];
			var isRoot = gene==newGenome.root;
			var parent = newGenome.FindParent(gene);
			var parentOp = parent as OperatorGene;

			var invalidOptions = new List<int>();
			var shouldNotRemove = ()=>isRoot || parent==null || parentOp==null;
			var doNotRemove = gene instanceof ParameterGene && shouldNotRemove();

			var lastOption = -1;

			while(invalidOptions!=null)
			{

				if(parent!=null && !parent.Children.Contains(gene))
					throw new Exception("Parent changed?");

				if(gene instanceof ConstantGene)
				{
					var cg = gene;
					switch(lastOption = Environment.NextRandomIntegerExcluding(2, invalidOptions))
					{
						// Simply alter the sign
						case 0:
							var abs = Math.Abs(cg.Multiple);

							if(abs>1 && Environment.Randomizer.Next(2)==0)
							{
								if(abs!=Math.Floor(abs) || Environment.Randomizer.Next(2)==0)
									cg.Multiple /= abs;
								else
									cg.Multiple -= (cg.Multiple/abs);
							}
							else
								cg.Multiple *= -1;

							invalidOptions = null;
							break;



						// Remove it!
						default:

							if(parentOp!=null)
							{
								parentOp.Remove(gene);
								invalidOptions = null;
							}
							break;

					}
				}

				else if(gene instanceof ParameterGene)
				{
					var pg = gene;
					switch(lastOption = Environment.NextRandomIntegerExcluding(doNotRemove
						? 4
						: 8, invalidOptions))
					{
						// Simply alter the sign
						case 0:
							var abs = Math.Abs(pg.Multiple);

							if(abs>1 && Environment.Randomizer.Next(2)==0)
								pg.Multiple /= abs;
							else
								pg.Multiple *= -1;

							invalidOptions = null;
							break;

						// Simply change parameters
						case 1:
							var nextParameter = Environment.NextRandomIntegerExcluding(InputParamCount, pg.ID);

							var newPG = new ParameterGene(nextParameter);
							if(isRoot)
								newGenome.Root = newPG;
							else
								parent.Replace(gene, newPG);

							invalidOptions = null;
							break;

						// Split it...
						case 2:
						{
							var newFn = OperatorGene.GetRandomOperation('/');

							if(isRoot)
								newGenome.Root = newFn;
							else
								parent.Replace(gene, newFn);

							newFn.Add(gene);
							newFn.Add(gene.Clone());

							invalidOptions = null;
							break;
						}

						// Apply a function
						case 3:
						{
							// Reduce the pollution of functions...
							if(Environment.Randomizer.Next(0, 3)!=0)
							{
								invalidOptions.Add(4);
								break;
							}

							var newFn = OperatorGene.GetRandomFunction();

							if(isRoot)
								newGenome.Root = newFn;
							else
								parent.Replace(gene, newFn);

							newFn.Add(gene);
							invalidOptions = null;
							break;
						}

						// Remove it!
						default:
							var children = parentOp.Children;
							if(parentOp.Count<3)
							{
								if(children.All(
										o => o instanceof ParameterGene || o instanceof ConstantGene))
									doNotRemove = true;
								else
								{
									var replacement = children.Where(
										o => o instanceof OperatorGene).Single();
									if(parentOp==newGenome.Root)
										newGenome.Root = replacement;
									else
										newGenome
											.FindParent(parentOp)
											.Replace(parentOp, replacement);

								}
							}

							if(!doNotRemove)
							{
								parentOp.ModifyValues(v=>v.Remove(gene));
								invalidOptions = null;
							}
							break;


					}
				}
				else if(gene instanceof OperatorGene)
				{
					var og = gene;
					if(OperatorGene.AvailableFunctions.Contains(og.Operator))
					{
						invalidOptions.Add(3);
						invalidOptions.Add(4);
					}

					switch(lastOption = Environment.NextRandomIntegerExcluding(doNotRemove
						? 6
						: 10, invalidOptions))
					{
						// Simply invert the sign
						case 0:
							og.Multiple *= -1;
							invalidOptions = null;
							break;

						// Simply change operations
						case 1:
							var currentOperatorIndex = OperatorGene.AvailableOperators.ToList().IndexOf(og.Operator);
							if(currentOperatorIndex== -1)
							{
								currentOperatorIndex
									= OperatorGene.AvailableFunctions.ToList().IndexOf(og.Operator);
								if(currentOperatorIndex!= -1)
								{
									if(OperatorGene.AvailableFunctions.Length==1)
									{
										invalidOptions.Add(1);
										break;
									}

									og.Operator = OperatorGene.AvailableFunctions[
										Environment.NextRandomIntegerExcluding(OperatorGene.AvailableFunctions.Length, currentOperatorIndex)
										];
								}


								break;
							}

							var newOperatorIndex = Environment.NextRandomIntegerExcluding(OperatorGene.AvailableOperators.Length, currentOperatorIndex);

							// Decide if we will also change the grouping.
							if(og.Count>2 && Environment.Randomizer.Next(og.Count)!=0)
							{
								var startIndex = Environment.Randomizer.Next(og.Count - 1);
								var endIndex = startIndex==0
									? Environment.Randomizer.Next(1, og.Count - 1)
									: Environment.Randomizer.Next(startIndex + 1, og.Count);

								og.ModifyValues(v =>
								{
									var contents = v.GetRange(startIndex, endIndex - startIndex);
									for(var o of contents)
									{
										v.Remove(o);
									}
									var O = OperatorGene.GetRandomOperation();
									O.AddRange(contents);
									v.Insert(startIndex, O);
								});
							}
							else // Grouping remains... Only operator changes.
								og.Operator = OperatorGene.AvailableOperators[newOperatorIndex];

							invalidOptions = null;
							break;


						// Add random parameter.
						case 2:
							// In order to avoid unnecessary reduction, avoid adding subsequent divisors.
							if(og.Operator=='/' && og.Count>1)
								break;

							og.Add(new ParameterGene(Environment.Randomizer.Next(InputParamCount)));
							invalidOptions = null;
							break;

						// Add random operator branch.
						case 3:


							var first = new ParameterGene(Environment.Randomizer.Next(InputParamCount));

							var newOp = InputParamCount==1
								? OperatorGene.GetRandomOperation('/')
								: OperatorGene.GetRandomOperation();

							newOp.Add(first);

							// Useless to divide a param by itself, avoid...
							if(newOp.Operator=='/')
								newOp.Add(new ParameterGene(Environment.NextRandomIntegerExcluding(InputParamCount, first.ID)));
							else
								newOp.Add(new ParameterGene(Environment.Randomizer.Next(InputParamCount)));

							og.Add(newOp);
							invalidOptions = null;
							break;
						// Apply a function
						case 4:
						{
							// Reduce the pollution of functions...
							if(Environment.Randomizer.Next(4)!=1)
							{
								break;
							}

							// Reduce the pollution of functions...
							if(OperatorGene.AvailableFunctions.Contains(og.Operator) && Environment.Randomizer.Next(4)!=1)
							{
								break;
							}

							var newFn = OperatorGene.GetRandomFunction();


							// Reduce the polution of functions...
							if(newFn.Operator==og.Operator)
							{
								if(Environment.Randomizer.Next(7)!=1)
								{
									invalidOptions.Add(5);
									break;
								}
							}

							if(isRoot)
								newGenome.Root = newFn;
							else
								parent.Replace(gene, newFn);

							newFn.Add(gene);
							invalidOptions = null;
							break;
						}
						case 5:
							if(og.Reduce())
								invalidOptions = null;
							break;
						// Remove it!
						default:
							if(OperatorGene.AvailableFunctions.Contains(og.Operator))
							{
								if(isRoot)
								{
									if(og.Children.Any())
										newGenome.Root = og.Children.First();
									else
									{
										doNotRemove = true;
										break;
									}

								}
								else
								{
									parentOp.ModifyValues(v =>
									{
										var index = v.IndexOf(gene);
										if(index!= -1)
										{
											v.InsertRange(index, og.Children);
											v.Remove(gene);
											invalidOptions = null;
										}
									});

								}
								break;
							}

							// Just like above, consider reduction instead of trimming...
							if(isRoot && og.Count>2)
							{
								og.ModifyValues(
									v => v.RemoveAt(Environment.Randomizer.Next(v.Count)));
							}
							else if(og.Count==2 && og.Children.Any(
									o => o instanceof OperatorGene) && og.Children.Any(
									o => o instanceof ParameterGene))
							{
								var childOpGene = og.Children.Single(
									o => o instanceof OperatorGene);
								og.Remove(childOpGene);
								if(isRoot)
									newGenome.Root = childOpGene;
								else
									parentOp.Replace(og, childOpGene);
							}
							else if(shouldNotRemove() || og.Count<3)
							{
								doNotRemove = true;
								break;
							}
							else
								parentOp.Remove(gene);

							invalidOptions = null;
							break;


					}
				}
			}

		}

		return newGenome;


	}

}
