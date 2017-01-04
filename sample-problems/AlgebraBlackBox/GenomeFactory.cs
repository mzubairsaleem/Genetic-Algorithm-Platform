using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using AlgebraBlackBox.Genes;
using Open;
using Open.Collections;

namespace AlgebraBlackBox
{

	public class GenomeFactory : GeneticAlgorithmPlatform.GenomeFactoryBase<Genome>
	{

		//noinspection JSMethodCanBeStatic
		protected Genome GenerateParamOnly(int id)
		{
			return Freeze(new Genome(new ParameterGene(id)));
		}

		//noinspection JSMethodCanBeStatic
		protected Genome GenerateOperated(int paramCount = 1)
		{
			var op = Operators.GetRandomOperationGene();
			var result = new Genome(op);

			for (var i = 0; i < paramCount; i++)
			{
				op.Add(new ParameterGene(i));
			}

			return Freeze(result);
		}

		public override Genome Generate(IEnumerable<Genome> source = null)
		{
			var attempts = 0;
			Genome genome = null;
			string hash = null;

			// Note: for now, we will only mutate by 1.

			// See if it's possible to mutate from the provided genomes.
			if (source != null && source.Any())
			{
				AttemptNewMutation(source, out genome);
			}

			if (genome == null)
			{
				for (uint m = 1; m < 4; m++)
				{

					// Establish a maximum.
					var tries = 10;
					var paramCount = 0;

					do
					{
						{ // Try a param only version first.
							genome = this.GenerateParamOnly(paramCount);
							hash = genome.Hash;
							attempts++;
							if (!_previousGenomes.ContainsKey(hash)) break;
						}

						paramCount += 1; // Operators need at least 2 params to start.

						{ // Then try an operator based version.
							genome = this.GenerateOperated(paramCount + 1);
							hash = genome.Hash;
							attempts++;
							if (!_previousGenomes.ContainsKey(hash)) break;
						}

						var t = Math.Min(_previousGenomes.Count * 2, 100); // A local maximum.
						do
						{
							genome = Mutate(_previousGenomes.Values.RandomSelectOne(), m);
							hash = genome.Hash;
							attempts++;
						}
						while (_previousGenomes.ContainsKey(hash) && --t != 0);

						// t==0 means nothing found :(
						if (t != 0) break;
					}
					while (--tries != 0);

					if (tries != 0)
						break;
					// else
					// 	genome = null; // Failed... Converged? No solutions? Saturated?
				}
			}

			//console.log("Generate attempts:",attempts);
			if (hash != null)
			{
				Genome temp;
				if (_previousGenomes.TryGetValue(hash, out temp))
				{
					return temp;
				}

				if (genome != null)
					_previousGenomes.TryAdd(hash, genome);
			}


			// if(!genome)
			// 	throw "Failed... Converged? No solutions? Saturated?";

			return genome;

		}

		public static Genome ApplyClone(Genome source, int geneIndex, Action<IGene, Genome> handler)
		{
			if (geneIndex == -1)
				throw new ArgumentOutOfRangeException("geneIndex", "Can't be -1.");
			var newGenome = source.Clone();
			var gene = newGenome.Genes[geneIndex];
			handler(gene, newGenome);
			return newGenome;
		}

		public static Genome ApplyClone(Genome source, int geneIndex, Action<IGene> handler)
		{
			if (geneIndex == -1)
				throw new ArgumentOutOfRangeException("geneIndex", "Can't be -1.");
			var newGenome = source.Clone();
			var gene = newGenome.Genes[geneIndex];
			handler(gene);
			return newGenome;
		}

		public static Genome ApplyClone(Genome source, IGene gene, Action<IGene, Genome> handler)
		{
			return ApplyClone(source, source.Genes.IndexOf(gene), handler);
		}

		public static Genome ApplyClone(Genome source, IGene gene, Action<IGene> handler)
		{
			return ApplyClone(source, source.Genes.IndexOf(gene), handler);
		}

		public static class VariationCatalog
		{

			public static Genome IncreaseMultipleMagnitude(Genome source, int geneIndex)
			{
				return ApplyClone(source, geneIndex, g =>
				{
					var absMultiple = Math.Abs(g.Multiple);
					g.Multiple += g.Multiple / absMultiple;
				});
			}

			public static Genome ReduceMultipleMagnitude(Genome source, int geneIndex)
			{
				return ApplyClone(source, geneIndex, g =>
				{
					var absMultiple = Math.Abs(g.Multiple);
					g.Multiple -= g.Multiple / absMultiple;
				});
			}

			public static bool CheckRemovalValidity(Genome source, IGene gene)
			{
				if (gene == source.Root) return false;
				// Validate worthyness.
				var parent = source.FindParent(gene);

				// Search for potential futility...
				// Basically, if there is no dynamic genes left after reduction then it's not worth removing.
				if (parent != source.Root && !parent.Where(g => g != gene && !(g is ConstantGene)).Any())
				{
					return CheckRemovalValidity(source, parent);
				}

				return false;
			}

			public static Genome RemoveGene(Genome source, int geneIndex)
			{
				// Validate worthyness.
				var gene = source.Genes[geneIndex];

				if (CheckRemovalValidity(source, gene))
				{
					return ApplyClone(source, geneIndex, (g, newGenome) =>
					{
						var parent = newGenome.FindParent(g);
						parent.Remove(g);
					});
				}
				return null;

			}
			public static Genome RemoveGene(Genome source, IGene gene)
			{
				return RemoveGene(source, source.Genes.IndexOf(gene));
			}

			public static bool CheckPromoteChildrenValidity(Genome source, IGene gene)
			{
				// Validate worthyness.
				var op = gene as GeneNode;
				return op != null && op.Count == 1;
			}

			// This should handle the case of demoting a function.
			public static Genome PromoteChildren(Genome source, int geneIndex)
			{
				// Validate worthyness.
				var gene = source.Genes[geneIndex];

				if (CheckPromoteChildrenValidity(source, gene))
				{
					return ApplyClone(source, geneIndex, (g, newGenome) =>
					{
						var op = (GeneNode)g;
						var child = op.Children.Single();
						op.Remove(child);
						newGenome.Replace(g, child);
					});
				}
				return null;

			}

			public static Genome PromoteChildren(Genome source, IGene gene)
			{
				return PromoteChildren(source, source.Genes.IndexOf(gene));
			}

			public static Genome ApplyFunction(Genome source, int geneIndex, char fn)
			{
				if (!Operators.Available.Functions.Contains(fn))
					throw new ArgumentException("Invalid function operator.", "fn");

				// Validate worthyness.
				return ApplyClone(source, geneIndex, (g, newGenome) =>
				{
					var newFn = Operators.New(fn);
					newGenome.Replace(g, newFn);
					newFn.Add(g);
				});

			}

			public static Genome ApplyFunction(Genome source, IGene gene, char fn)
			{
				return ApplyFunction(source, source.Genes.IndexOf(gene), fn);
			}
		}

		public static class MutationCatalog
		{
			public static Genome MutateSign(Genome source, IGene gene, int options = 3)
			{
				var isRoot = source.Root == gene;
				var parentIsSquareRoot = source.FindParent(gene) is SquareRootGene;
				return ApplyClone(source, gene, g =>
				{
					switch (RandomUtilities.Random.Next(options))
					{
						case 0:
							// Alter Sign
							g.Multiple *= -1;
							if (parentIsSquareRoot)
							{
								// Sorry, not gonna mess with unreal (sqrt neg numbers yet).
								if (RandomUtilities.Random.Next(2) == 0)
									goto case 1;
								else
									goto case 2;
							}
							break;
						case 1:
							// Don't zero the root or make the internal multiple negative.
							if (isRoot && g.Multiple == +1 || parentIsSquareRoot && g.Multiple <= 0)
								goto case 2;
							// Decrease multiple.
							g.Multiple -= 1;
							break;
						case 2:
							// Don't zero the root. (makes no sense)
							if (isRoot && g.Multiple == -1)
								goto case 1;
							// Increase multiple.
							g.Multiple += 1;
							break;

					}
				});
			}

			public static Genome MutateParameter(Genome source, ParameterGene gene)
			{
				var inputParamCount = source.Genes.OfType<ParameterGene>().GroupBy(p => p.ToString()).Count();
				return ApplyClone(source, gene, (g, newGenome) =>
				{
					var parameter = (ParameterGene)g;
					var nextParameter = RandomUtilities.NextRandomIntegerExcluding(inputParamCount + 1, parameter.ID);
					newGenome.Replace(g, new ParameterGene(nextParameter, parameter.Multiple));
				});
			}

			public static Genome ChangeOperation(Genome source, OperatorGeneBase gene)
			{
				bool isFn = gene is FunctionGene;
				if (isFn)
				{
					// Functions with no other options?
					if (Operators.Available.Functions.Count < 2)
						return null;
				}
				else
				{
					// Never will happen, but logic states that this is needed.
					if (Operators.Available.Operators.Count < 2)
						return null;
				}

				return ApplyClone(source, gene, (g, newGenome) =>
				{
					var og = (OperatorGeneBase)g;
					OperatorGeneBase replacement = isFn
						? Operators.GetRandomFunctionGene(og.Operator)
						: Operators.GetRandomOperationGene(og.Operator);
					replacement.AddThese(og.Children);
					og.Clear();
					newGenome.Replace(g, replacement);
				});
			}

			public static Genome AddParameter(Genome source, OperatorGeneBase gene)
			{
				bool isFn = gene is FunctionGene;
				if (isFn)
				{
					// Functions with no other options?
					if (gene is SquareRootGene || gene is DivisionGene)
						return null;
				}

				var inputParamCount = source.Genes.OfType<ParameterGene>().GroupBy(p => p.ToString()).Count();
				return ApplyClone(source, gene, (g, newGenome) =>
				{
					var og = (OperatorGeneBase)g;
					og.Add(new ParameterGene(RandomUtilities.Random.Next(inputParamCount + 1)));
				});
			}

			public static Genome BranchOperation(Genome source, OperatorGeneBase gene)
			{
				var inputParamCount = source.Genes.OfType<ParameterGene>().GroupBy(p => p.ToString()).Count();
				return ApplyClone(source, gene, (g, newGenome) =>
				{
					var n = new ParameterGene(RandomUtilities.Random.Next(inputParamCount));
					var newOp = Operators.GetRandomOperationGene();

					if (gene is FunctionGene || RandomUtilities.Random.Next(4) == 0)
					{
						var index = RandomUtilities.Random.Next(2);
						if (index == 1)
						{
							newOp.Add(n);
							newOp.Add(g);
						}
						else
						{
							newOp.Add(g);
							newOp.Add(n);
						}
						newGenome.Replace(g, newOp);
					}
					else
					{
						newOp.Add(n);
						// Useless to divide a param by itself, avoid...
						newOp.Add(new ParameterGene(RandomUtilities.Random.Next(inputParamCount)));

						((OperatorGeneBase)g).Add(newOp);
					}

				});
			}

			public static Genome Square(Genome source, IGene gene)
			{
				if (source.FindParent(gene) is SquareRootGene)
					return null;

				return ApplyClone(source, gene, (g, newGenome) =>
				{
					var newFn = new ProductGene(g.Multiple);
					g.Multiple = 1;
					newFn.Add(g);
					newFn.Add(g.Clone());
					newGenome.Replace(g, newFn);
				});
			}

		}

		protected IEnumerable<Genome> GenerateVariationsUnfiltered(Genome source)
		{
			var sourceGenes = source.Genes;
			var count = sourceGenes.Length;

			for (var i = 0; i < count; i++)
			{
				var gene = sourceGenes[i];
				var isRoot = gene == source.Root;
				yield return VariationCatalog.RemoveGene(source, i);
			}

			for (var i = 0; i < count; i++)
			{
				var gene = sourceGenes[i];
				var isRoot = gene == source.Root;
				yield return VariationCatalog.ReduceMultipleMagnitude(source, i);
			}

			for (var i = 0; i < count; i++)
			{
				var gene = sourceGenes[i];
				var isRoot = gene == source.Root;

				yield return VariationCatalog.PromoteChildren(source, i);

				// foreach (var fn in Operators.Available.Functions)
				// {
				// 	yield return VariationCatalog.ApplyFunction(source, i, fn);
				// }
			}

			for (var i = 0; i < count; i++)
			{
				var gene = sourceGenes[i];
				var isRoot = gene == source.Root;
				yield return VariationCatalog.IncreaseMultipleMagnitude(source, i);
			}

		}
		protected IEnumerable<Genome> GenerateVariations(Genome source)
		{
			return GenerateVariationsUnfiltered(source)
				.Where(genome => genome != null)
				.Select(genome =>
				{
					genome = genome.AsReduced();
					Genome temp;
					return _previousGenomes.TryGetValue(genome.ToString(), out temp) ? temp : Freeze(genome);
				})
				.GroupBy(g => g.Hash)
				.Select(g => g.First());
		}

		Genome Freeze(Genome target)
		{
			if (target == null) return null;
			target.RegisterVariations(GenerateVariations(target));
			target.RegisterMutations(Mutator(target));
			target.Freeze();
			return target;
		}

		// Keep in mind that Mutation is more about structure than 'variations' of multiples and constants.
		private Genome MutateUnfrozen(Genome target)
		{
			/* Possible mutations:
			 * 1) Adding a parameter node to an operation.
			 * 2) Apply a function to node.
			 * 3) Adding an operator and a parameter node.
			 * 4) Removing a node from an operation.
			 * 5) Removing an operation.
			 * 6) Removing a function. */

			var genes = target.Genes;

			while (genes.Any())
			{
				var gene = genes.RandomSelectOne();
				if (gene is ConstantGene)
				{
					switch (RandomUtilities.Random.Next(4))
					{
						case 0:
							return VariationCatalog
								.ApplyFunction(target, gene, Operators.GetRandomFunction());
						case 1:
							return MutationCatalog
								.MutateSign(target, gene, 1);
						default:
							return VariationCatalog
								.RemoveGene(target, gene);
					}

				}
				else if (gene is ParameterGene)
				{
					var options = Enumerable.Range(0, 5).ToList();
					while (options.Any())
					{
						switch (options.RandomPluck())
						{
							case 0:
								return MutationCatalog
									.MutateSign(target, gene, 1);

							// Simply change parameters
							case 1:
								return MutationCatalog
									.MutateParameter(target, (ParameterGene)gene);

							// Apply a function
							case 2:
								// Reduce the pollution of functions...
								if (RandomUtilities.Random.Next(0, 4) == 0)
								{
									return VariationCatalog
										.ApplyFunction(target, gene, Operators.GetRandomFunction());
								}
								break;

							// Split it...
							case 3:
								if (RandomUtilities.Random.Next(0, 4) == 0)
								{
									return MutationCatalog
											.Square(target, gene);
								}
								break;

							// Remove it!
							default:
								var attempt = VariationCatalog.RemoveGene(target, gene);
								if (attempt != null)
									return attempt;
								break;

						}
					}


				}
				else if (gene is OperatorGeneBase)
				{
					var options = Enumerable.Range(0, 8).ToList();
					while (options.Any())
					{
						Genome ng = null;
						switch (options.RandomPluck())
						{
							case 0:
								ng = MutationCatalog
									.MutateSign(target, gene, 1);
								break;

							case 1:
								ng = VariationCatalog
									.PromoteChildren(target, gene);
								break;

							case 2:
								ng = MutationCatalog
									.ChangeOperation(target, (OperatorGeneBase)gene);
								break;

							// Apply a function
							case 3:
								// Reduce the pollution of functions...
								if (RandomUtilities.Random.Next(0, gene is FunctionGene ? 6 : 4) == 0)
								{
									var f = Operators.GetRandomFunction();
									// Function of function? Reduce probability even further. Coin toss.
									if (f.GetType() != gene.GetType() || RandomUtilities.Random.Next(2) == 0)
										return VariationCatalog
										.ApplyFunction(target, gene, f);

								}
								break;

							case 4:
								ng = VariationCatalog.RemoveGene(target, gene);
								break;

							case 5:
								ng = MutationCatalog
									.AddParameter(target, (OperatorGeneBase)gene);
								break;

							case 6:
								ng = MutationCatalog
									.BranchOperation(target, (OperatorGeneBase)gene);
								break;

							case 7:
								// This has a potential to really bloat the function so allow, but very sparingly.
								if (RandomUtilities.Random.Next(0, 4) == 0)
								{
									return MutationCatalog
										.Square(target, gene);
								}
								break;

						}

						if (ng != null)
							return ng;
					}



				}

			}

			return null;

		}


		protected override Genome MutateInternal(Genome target)
		{
			return Freeze(MutateUnfrozen(target));
		}

		public override void Generate(uint count)
		{
			for (uint i = 0; i < count; i++)
				Queue.Post(Generate());
		}

	}
}