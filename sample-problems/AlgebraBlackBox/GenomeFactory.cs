using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraBlackBox
{
	class GenomeFactory
	{

		public int InputParamCount
		{
			get;
			private set;
		}

		public GenomeFactory(int inputParams, int maxGenomeTracking = 1000)
		{
			InputParamCount = inputParams;
			MaxGenomeTracking = maxGenomeTracking;
		}

		public int MaxGenomeTracking
		{
			get;
			set;
		}

		List<Genome> _previousGenomes = new List<Genome>();
		public IReadOnlyCollection<Genome> PreviousGenomes
		{
			get
			{
				return _previousGenomes.AsReadOnly();
			}
		}

		public void TrimPreviousGenomes()
		{
			while (_previousGenomes.Count > MaxGenomeTracking)
				_previousGenomes.RemoveAt(0);
		}

		protected Genome GenerateSimple()
		{
			var result = new Genome();
			var op = new OperatorGene();
			op.Operator = OperatorGene.GetRandomOperator();
			result.Root = op;

			for (var i = 0; i < InputParamCount; i++)
				op.Add(new ParameterGene(i));

			return result;
		}

		public Genome Generate()
		{
			int tries = 0;

			var genome = GenerateSimple();
			var gs = genome.CachedToStringReduced;
			while (_previousGenomes.Any(g => g.CachedToStringReduced == gs) && ++tries < 1000)
			{
				genome = Mutate(_previousGenomes[Environment.Randomizer.Next(_previousGenomes.Count)]);
				gs = genome.CachedToStringReduced;
				// Is it there some weird possibility that this could get stuck?
			}

			if (tries >= 1000)
				return null; // Failed... Converged? No solutions? Saturated?

			_previousGenomes.Add(genome);

			return genome;
		}

		public Genome GenerateFrom(IEnumerable<Organism> source)
		{
			var sourceGenomes = source.OrderBy(s => s.FitnessAverage).Select(s => s.Genome).ToList();

			// Use a fitness weighted random selection.
			var selectionlist = new List<Genome>();
			for (var i = 0; i < sourceGenomes.Count; i++)
			{
				var g = sourceGenomes[i];
				for (var n = 0; n < i + 1; n++)
					selectionlist.Add(g);
			}

			int tries = 0;
			Genome genome;
			string gs;
			do
			{
				genome = Mutate(selectionlist[Environment.Randomizer.Next(selectionlist.Count)]);
				gs = genome.CachedToStringReduced;
				// Is it there some weird possibility that this could get stuck?
			}
			while (_previousGenomes.Any(g => g.CachedToStringReduced == gs) && ++tries < 1000);

			if (tries >= 1000)
				return null; // Failed... Converged? No solutions? Saturated?

			_previousGenomes.Add(genome);

			return genome;
		}

		public Genome Mutate(Genome source, int mutations = 1)
		{
			/* Possible mutations:
			 * 1) Adding a parameter node to an operation.
			 * 2) Apply a funciton to node.
			 * 3) Adding an operator and a parameter node.
			 * 4) Removing a parameter from an operation.
			 * 5) Removing an operation.
			 * 6) Removing a function. */

			var newGenome = source.Clone();

			for (var i = 0; i < mutations; i++)
			{
				// First randomly select the gene to mutate.
				var genes = newGenome.Genes.ToArray();
				var g = Environment.Randomizer.Next(genes.Length);
				var gene = genes[g];
				var isRoot = gene == newGenome.Root;
				var parent = newGenome.FindParent(gene);
				var parentOp = parent as OperatorGene;

				var invalidOptions = new List<int>();
				Func<bool> shouldntRemove = ()=>isRoot || parent == null || parentOp == null;
				var dontRemove = gene is ParameterGene && shouldntRemove();

				int lastOption = -1;

				while (invalidOptions != null)
				{

					if (parent != null && !parent.Children.Contains(gene))
						throw new Exception("Parent changed?");

					if (gene is ConstantGene)
					{
						var cg = (ConstantGene)gene;
						switch (lastOption = Environment.NextRandomIntegerExcluding(2, invalidOptions))
						{
							// Simply alter the sign
							case 0:
								var abs = Math.Abs(cg.Multiple);

								if (abs > 1 && Environment.Randomizer.Next(2) == 0) {
									if (abs != Math.Floor(abs) || Environment.Randomizer.Next(2) == 0)
										cg.Multiple /= abs;
									else
										cg.Multiple -= (cg.Multiple / abs);
								}
								else
									cg.Multiple *= -1;

								invalidOptions = null;
								break;



							// Remove it!
							default:

								if (parentOp!=null)
								{
									parentOp.Remove(gene);
									invalidOptions = null;
								}
								break;

						}
					}

					else if (gene is ParameterGene)
					{
						var pg = (ParameterGene)gene;
						switch (lastOption = Environment.NextRandomIntegerExcluding(dontRemove?4:8, invalidOptions))
						{
							// Simply alter the sign
							case 0:
								var abs = Math.Abs(pg.Multiple);

								if (abs > 1 && Environment.Randomizer.Next(2)==0)
									pg.Multiple /= abs;
								else 
									pg.Multiple *= -1;
								
								invalidOptions = null;
								break;

							// Simply change parameters
							case 1:
								var nextParameter = Environment.NextRandomIntegerExcluding(InputParamCount, pg.ID);

								var newpg = new ParameterGene(nextParameter);
								if (isRoot)
									newGenome.Root = newpg;
								else
									parent.Replace(gene, newpg);

								invalidOptions = null;
								break;

							// Split it...
							case 2:
								{
									var newFn = OperatorGene.GetRandomOperation('/');

									if (isRoot)
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
									// Reduce the polution of functions...
									if (Environment.Randomizer.Next(0, 3) != 0)
									{
										invalidOptions.Add(4);
										break;
									}

									var newFn = OperatorGene.GetRandomFunction();

									if (isRoot)
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
								if (parentOp.Count < 3) {
									if (children.All(o => o is ParameterGene || o is ConstantGene))
										dontRemove = true;
									else
									{
										var replacement = children.Where(o => o is OperatorGene).Single();
										if (parentOp == newGenome.Root)
											newGenome.Root = replacement;
										else
											newGenome
												.FindParent(parentOp)
												.Replace(parentOp, replacement);

									}
								}

								if (!dontRemove) { 
									parentOp.ModifyValues(v=>v.Remove(gene));
									invalidOptions = null;
								}
								break;


						}
					}
					else if (gene is OperatorGene)
					{
						var og = (OperatorGene)gene;
						if (OperatorGene.AvailableFunctions.Contains(og.Operator))
						{
							invalidOptions.Add(3);
							invalidOptions.Add(4);
						}

						switch (lastOption = Environment.NextRandomIntegerExcluding(dontRemove?6:10, invalidOptions))
						{
							// Simply invert the sign
							case 0:
								og.Multiple *= -1;
								invalidOptions = null;
								break;

							// Simply change operations
							case 1:
								var currentOperatorIndex = OperatorGene.AvailableOperators.ToList().IndexOf(og.Operator);
								if (currentOperatorIndex == -1)
								{
									currentOperatorIndex = OperatorGene.AvailableFunctions.ToList().IndexOf(og.Operator);
									if (currentOperatorIndex != -1)
									{
										if (OperatorGene.AvailableFunctions.Length == 1)
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
								if (og.Count > 2 && Environment.Randomizer.Next(og.Count) != 0)
								{
									int startIndex = Environment.Randomizer.Next(og.Count - 1);
									int endIndex = startIndex == 0
										? Environment.Randomizer.Next(1, og.Count - 1)
										: Environment.Randomizer.Next(startIndex + 1, og.Count);

									og.ModifyValues(v =>
									{
										var contents = v.GetRange(startIndex, endIndex - startIndex);
										foreach (var o in contents)
											v.Remove(o);
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
								// In order to avoid unecessary reduction, avoid adding subsequent divisors.
								if (og.Operator == '/' && og.Count > 1)
									break;

								og.Add(new ParameterGene(Environment.Randomizer.Next(InputParamCount)));
								invalidOptions = null;
								break;

							// Add random operator branch.
							case 3:


								var first = new ParameterGene(Environment.Randomizer.Next(InputParamCount));

								var newOp = InputParamCount == 1
									? OperatorGene.GetRandomOperation('/')
									: OperatorGene.GetRandomOperation();

								newOp.Add(first);

								// Useless to divide a param by itself, avoid...
								if (newOp.Operator == '/')
									newOp.Add(new ParameterGene(Environment.NextRandomIntegerExcluding(InputParamCount, first.ID)));
								else
									newOp.Add(new ParameterGene(Environment.Randomizer.Next(InputParamCount)));

								og.Add(newOp);
								invalidOptions = null;
								break;
							// Apply a function
							case 4:
								{
									// Reduce the polution of functions...
									if (Environment.Randomizer.Next(4) != 1)
									{
										break;
									}

									// Reduce the polution of functions...
									if (OperatorGene.AvailableFunctions.Contains(og.Operator) && Environment.Randomizer.Next(4) != 1)
									{
										break;
									}

									var newFn = OperatorGene.GetRandomFunction();


									// Reduce the polution of functions...
									if (newFn.Operator==og.Operator)
									{
										if (Environment.Randomizer.Next(7) != 1)
										{ 
											invalidOptions.Add(5);
											break;
										}
									}

									if (isRoot)
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
								if (OperatorGene.AvailableFunctions.Contains(og.Operator))
								{
									if (isRoot)
									{
										if (og.Children.Any())
											newGenome.Root = og.Children.First();
										else
										{
											dontRemove = true;
											break;
										}

									}
									else 
									{
										parentOp.ModifyValues(v =>
										{
											var index = v.IndexOf(gene);
											if (index != -1)
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
								if (isRoot && og.Count > 2)
								{
									og.ModifyValues(v => v.RemoveAt(Environment.Randomizer.Next(v.Count)));
								}
								else if (og.Count == 2 && og.Children.Any(o => o is OperatorGene) && og.Children.Any(o => o is ParameterGene))
								{
									var childOpGene = og.Children.Single(o => o is OperatorGene);
									og.Remove(childOpGene);
									if (isRoot)
										newGenome.Root = childOpGene;
									else
										parentOp.Replace(og, childOpGene);
								}
								else if(shouldntRemove() || og.Count<3)
								{
									dontRemove = true;
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
}
