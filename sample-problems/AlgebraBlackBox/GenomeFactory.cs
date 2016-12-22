using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlgebraBlackBox.Genes;

namespace AlgebraBlackBox
{

    delegate bool GeneHandler(IGene gene, Genome newGenome);

    public class GenomeFactory : GeneticAlgorithmPlatform.GenomeFactoryBase<Genome>
    {

        //noinspection JSMethodCanBeStatic
        protected Genome GenerateParamOnly(int id)
        {
            return new Genome(new ParameterGene(id));
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

            return result;
        }


        public override async Task<Genome> Generate(IEnumerable<Genome> source = null)
        {

            var attempts = 0;
            Genome genome = null;
            string hash = null;
            using (var p = _previousGenomes.Values.Memoize())
            {
                // Note: for now, we will only mutate by 1.

                // See if it's possible to mutate from the provided genomes.
                if (source != null && source.Any())
                {
                    // Find one that will mutate well and use it.
                    for (uint m = 1; m < 4; m++)
                    {
                        var tries = 10;//200;
                        do
                        {
                            genome = await Mutate(source.RandomSelectOne(), m);
                            hash = genome.Hash;
                            attempts++;
                        }
                        while (_previousGenomes.ContainsKey(hash) && --tries != 0);

                        if (tries != 0)
                            break;
                        // else
                        // 	genome = null; // Failed... Converged? No solutions? Saturated?
                    }

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
                                genome = await Mutate(p.RandomSelectOne(), m);
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
            }
            //console.log("Generate attempts:",attempts);
            if (hash != null)
            {
                if (_previousGenomes.TryGetValue(hash, out genome))
                {
                    return genome;
                }

                if (genome != null)
                    _previousGenomes.TryAdd(hash, genome);
            }


            // if(!genome)
            // 	throw "Failed... Converged? No solutions? Saturated?";

            return genome;

        }

        public override Task<IEnumerable<Genome>> GenerateVariations(Genome source)
        {
            return new Task<IEnumerable<Genome>>(() =>
            {
                var result = new List<Genome>();
                var sourceGenes = source.Genes.ToArray();
                var count = sourceGenes.Length;
                // for (var i = 0; i < count; i++)
                // {
                //     var gene = sourceGenes[i];
                //     var isRoot = gene == source.Root;

                //     Action<GeneHandler> applyClone = (GeneHandler handler) =>
                //     {
                //         var newGenome = source.Clone();
                //         if (handler(newGenome.Genes.ElementAt(i), newGenome) != false)
                //             result.Add(newGenome);
                //     };

                //     var absMultiple = Math.Abs(gene.Multiple);
                //     if (absMultiple > 1)
                //     {
                //         applyClone((g, newGenome) =>
                //                     {
                //                         g.Multiple -= g.Multiple / absMultiple;
                //                         return true;
                //                     });
                //     }

                //     var parentOp = source.FindParent(gene) as OperatorGeneBase;
                //     if (parentOp != null)
                //     {
                //         if (parentOp.Count > 1)
                //         {
                //             applyClone((g, newGenome) =>
                //                         {
                //                             var parentOp2 = newGenome.FindParent(g) as OperatorGeneBase;
                //                             parentOp2.Remove(g);

                //                 // Reduce to avoid NaN.
                //                 if (parentOp2.Count == 1 && Operators.Available.Operators.Contains(parentOp2.Operator))
                //                             {
                //                                 var grandParent = newGenome.FindParent(parentOp);
                //                                 if (grandParent != null)
                //                                 {
                //                                     var grandChild = parentOp.Children.Single();
                //                                     grandChild.Multiple *= parentOp.Multiple;
                //                                     parentOp.Remove(grandChild);
                //                                     grandParent.Replace(parentOp, grandChild);
                //                                 }
                //                             }
                //                             return true;
                //                         });
                //         }
                //     }

                //     var opGene = gene as OperatorGeneBase;
                //     if (opGene != null && opGene.Count == 1)
                //     {
                //         applyClone((ng, newGenome) =>
				// 		{

				// 			var child = ((GeneNode)ng).First();
				// 			var parentOp2 = newGenome.FindParent(ng);

				// 			if (isRoot)
				// 			{
				// 				// If the root operator is a function, swap it's contents for the root.
				// 				newGenome.Root = child;
				// 			}
				// 			else
				// 			{
				// 				var pGenes = parentOp2.ToArray();
				// 				parentOp2.Clear();
				// 				foreach(var g in pGenes)
				// 				{
				// 					parentOp2.Add(g == ng ? child : g);
				// 				}
				// 			}

				// 			return true;
				// 		});

                //         if (Operators.Available.Functions.Contains(opGene.Operator))
                //         {
                //             applyClone((g, newGenome) =>
				// 			{

				// 				((OperatorGeneBase)g).Operator = Operators.ADD;
				// 				return true;
				// 			});
                //         }


                //     }
                // }

				// if (source.Root is OperatorGene && Operators.Available.Functions.Contains(source.Root.Operator))
                // {
                //     // Try it without a function!

                //     var newGenome = source.Clone();
                //     var first = newGenome.Root.get(0);
                //     newGenome.Root.Remove(first);
                //     newGenome.Root = first;
                //     result.Add(newGenome);
                // }
                // else
                // {
                //     // Try it with a function!

                //     foreach (var op in Operators.Available.Functions)
                //     {
                //         var newGenome = source.Clone();
                //         var newFn = Operators.New(op);
                //         newFn.Add(newGenome.Root);
                //         newGenome.Root = newFn;
                //         result.Add(newGenome);
                //     }
                // }

                return result
                            //.filter(genome => !_previousGenomes.ContainsKey(genome.hash))
                            .Select(genome =>
                            {
                                Genome temp;
                                if (_previousGenomes.TryGetValue(genome.Hash, out temp))
                                    genome = temp;
                                genome = genome.AsReduced();
                                if (_previousGenomes.TryGetValue(genome.Hash, out temp))
                                    genome = temp;
                                return genome;
                            })
                            // //.filter(genome => !_previousGenomes.ContainsKey(genome.hash))
                            ;
            });
        }

        public override Task<Genome> Mutate(Genome source, uint mutations = 1)
        {
            return new Task<Genome>(() =>
            {
                var inputParamCount = source.Genes.OfType<ParameterGene>().Count();

                /* Possible mutations:
                 * 1) Adding a parameter node to an operation.
                 * 2) Apply a function to node.
                 * 3) Adding an operator and a parameter node.
                 * 4) Removing a node from an operation.
                 * 5) Removing an operation.
                 * 6) Removing a function. */

                var newGenome = source.Clone();

                // for (var i = 0; i < mutations; i++)
                // {
                //     // First randomly select the gene to mutate.
                //     var genes = newGenome.Genes.ToArray();
                //     var gene = genes.RandomSelectOne();
                //     var isRoot = gene == newGenome.Root;
                //     var parent = newGenome.FindParent(gene);
                //     var parentOp = parent as OperatorGeneBase;

                //     var invalidOptions = new HashSet<int>();
                //     Func<bool> shouldNotRemove = () => isRoot || parent == null || parentOp == null;
                //     var doNotRemove = gene is ParameterGene && shouldNotRemove();

                //     var lastOption = -1;

                //     while (invalidOptions != null)
                //     {

                //         if (parent != null && !parent.Contains(gene))
                //             throw new Exception("Parent changed?");

                //         if (gene is ConstantGene)
                //         {
                //             var cg = gene;
                //             switch (lastOption = RandomUtil.NextRandomIntegerExcluding(2, invalidOptions))
                //             {
                //                 // Simply alter the sign
                //                 case 0:
                //                     var abs = Math.Abs(cg.Multiple);

                //                     if (abs > 1 && RandomUtil.Random.Next(2) == 0)
                //                     {
                //                         if (abs != Math.Floor(abs) || RandomUtil.Random.Next(2) == 0)
                //                             cg.Multiple /= abs;
                //                         else
                //                             cg.Multiple -= (cg.Multiple / abs);
                //                     }
                //                     else
                //                         cg.Multiple *= -1;

                //                     invalidOptions = null;
                //                     break;



                //                 // Remove it!
                //                 default:

                //                     if (parentOp != null)
                //                     {
                //                         parentOp.Remove(gene);
                //                         invalidOptions = null;
                //                     }
                //                     break;

                //             }
                //         }

                //         else if (gene is ParameterGene)
                //         {
                //             var pg = gene as ParameterGene;
                //             switch (lastOption = RandomUtil.NextRandomIntegerExcluding(doNotRemove
                //                 ? 4
                //                 : 6, invalidOptions))
                //             {
                //                 // Simply alter the sign
                //                 case 0:
                //                     {
                //                         var abs = Math.Abs(pg.Multiple);

                //                         if (abs > 1 && RandomUtil.Random.Next(2) == 0)
                //                             pg.Multiple /= abs;
                //                         else
                //                             pg.Multiple *= -1;

                //                         invalidOptions = null;
                //                         break;
                //                     }

                //                 // Simply change parameters
                //                 case 1:
                //                     var nextParameter = RandomUtil.NextRandomIntegerExcluding(inputParamCount + 1, pg.ID);

                //                     var newPG = new ParameterGene(nextParameter);
                //                     if (isRoot)
                //                         newGenome.Root = newPG;
                //                     else
                //                         parent.Replace(gene, newPG);

                //                     invalidOptions = null;
                //                     break;

                //                 // Split it...
                //                 case 2:
                //                     {
                //                         var newFn = Operators.GetRandomOperationGene(Operators.DIVIDE); // excluding divide

                //                         if (isRoot)
                //                             newGenome.Root = newFn;
                //                         else
                //                             parent.Replace(gene, newFn);

                //                         newFn.Add(gene);
                //                         newFn.Add(gene.Clone());

                //                         invalidOptions = null;
                //                         break;
                //                     }

                //                 // Apply a function
                //                 case 3:
                //                     {
                //                         // Reduce the pollution of functions...
                //                         if (RandomUtil.Random.Next(0, 4) != 0)
                //                         {
                //                             invalidOptions.Add(4);
                //                             break;
                //                         }

                //                         var newFn = Operators.GetRandomFunctionGene();

                //                         if (isRoot)
                //                             newGenome.Root = newFn;
                //                         else
                //                             parent.Replace(gene, newFn);

                //                         newFn.Add(gene);
                //                         invalidOptions = null;
                //                         break;
                //                     }

                //                 // Remove it!
                //                 default:
                //                     if (parentOp == null)
                //                         throw new Exception("Missing parent operator");

                //                     if (parentOp.Count < 3)
                //                     {
                //                         if (parentOp.All(
                //                                 o => o is ParameterGene || o is ConstantGene))
                //                             doNotRemove = true;
                //                         else
                //                         {
                //                             var replacement = parentOp
                //                                 .OfType<OperatorGeneBase>()
                //                                 .Single();

                //                             if (parentOp == newGenome.Root)
                //                                 newGenome.Root = replacement;
                //                             else
                //                                 newGenome
                //                                     .FindParent(parentOp)
                //                                     .Replace(parentOp, replacement);

                //                         }
                //                     }

                //                     if (!doNotRemove)
                //                     {
                //                         parentOp.Remove(gene);
                //                         invalidOptions = null;

                //                         // console.log(
                //                         // 	convertParameterToAlphabet(source.toString()),
                //                         // 	convertParameterToAlphabet(gene.toString()),
                //                         // 	convertParameterToAlphabet(newGenome.toString())
                //                         // );

                //                     }
                //                     break;


                //             }
                //         }
                //         else if (gene is OperatorGeneBase)
                //         {
                //             var og = gene as OperatorGeneBase;
                //             var isFn = gene is FunctionGene;
                //             if (isFn)
                //             {
                //                 if (!isRoot) // Might need to break it out.
                //                     invalidOptions.Add(3);
                //                 invalidOptions.Add(4);
                //             }

                //             switch (lastOption = RandomUtil.NextRandomIntegerExcluding(doNotRemove
                //                 ? 6
                //                 : 10, invalidOptions))
                //             {
                //                 // Simply invert the sign
                //                 case 0:
                //                     {
                //                         gene.Multiple *= -1;
                //                         invalidOptions = null;
                //                         break;
                //                     }

                //                 // Simply change operations
                //                 case 1:
                //                     {
                //                         var currentOperatorIndex
                //                                 = Operators.Available.Operators.IndexOf(og.Operator);
                //                         if (currentOperatorIndex == -1)
                //                         {
                //                             currentOperatorIndex
                //                                 = Operators.Available.Functions.IndexOf(og.Operator);
                //                             if (currentOperatorIndex != -1)
                //                             {
                //                                 if (Operators.Available.Functions.Count == 1)
                //                                 {
                //                                     invalidOptions.Add(1);
                //                                     break;
                //                                 }

                //                                 gene.Operator = Operators.Available.Functions[
                //                                     RandomUtil.NextRandomIntegerExcluding(Operators.Available.Functions.Count, currentOperatorIndex)];
                //                             }


                //                             break;
                //                         }

                //                         var newOperatorIndex = RandomUtil.NextRandomIntegerExcluding(Operators.Available.Operators.Count, currentOperatorIndex);

                //                         // Decide if we will also change the grouping.
                //                         if (gene.Count > 2 && RandomUtil.Random.Next(gene.Count) != 0)
                //                         {
                //                             var startIndex = RandomUtil.Random.Next(gene.Count - 1);
                //                             var endIndex = startIndex == 0
                //                                 ? RandomUtil.Random.Next.inRange(1, gene.Count - 1)
                //                                 : RandomUtil.Random.Next.inRange(startIndex + 1, gene.Count);

                //                             gene.Sync.Modifying(() =>
                //                             {
                //                                 var contents = gene
                //                                     .Skip(startIndex)
                //                                     .Take(endIndex - startIndex)
                //                                     .ToArray();

                //                                 foreach (var o in contents)
                //                                 {
                //                                     gene.Remove(o);
                //                                 }

                //                                 var O = OperatorGene.GetRandomOperationGene();
                //                                 O.Add(contents);
                //                                 gene.Insert(startIndex, O);

                //                                 return true;
                //                             });
                //                         }
                //                         else // Grouping remains... Only operator changes.
                //                             gene.Operator = Operators.Available.Operators[newOperatorIndex];

                //                         invalidOptions = null;
                //                         break;
                //                     }

                //                 // Add random parameter.
                //                 case 2:
                //                     {
                //                         if (gene.Operator == Operator.SQUARE_ROOT
                //                             // In order to avoid unnecessary reduction, avoid adding subsequent divisors.
                //                             || gene.Operator == Operator.DIVIDE && gene.Count > 1)
                //                             break;

                //                         gene.add(new ParameterGene(RandomUtil.Random.Next(inputParamCount)));
                //                         invalidOptions = null;
                //                         break;
                //                     }

                //                 // Add random operator branch.
                //                 case 3:
                //                     {
                //                         var n = new ParameterGene(RandomUtil.Random.Next(inputParamCount));

                //                         var newOp = inputParamCount <= 1
                //                             ? OperatorGene.getRandomOperation('/')
                //                             : OperatorGene.getRandomOperation();

                //                         if (isFn || RandomUtil.Random.Next(4) == 0)
                //                         {
                //                             // logic states that this MUST be the root node.
                //                             var index = RandomUtil.Random.Next(2);
                //                             if (index)
                //                             {
                //                                 newOp.add(n);
                //                                 newOp.add(newGenome.root);
                //                             }
                //                             else
                //                             {
                //                                 newOp.add(newGenome.root);
                //                                 newOp.add(n);
                //                             }

                //                             newGenome.root = newOp;
                //                         }
                //                         else
                //                         {

                //                             newOp.add(n);

                //                             // Useless to divide a param by itself, avoid...
                //                             if (newOp.Operator == Operators.DIVIDE)
                //                                 newOp.Add(new ParameterGene(RandomUtil.NextRandomIntegerExcluding(inputParamCount, n.id)));
                //                             else
                //                                 newOp.Add(new ParameterGene(RandomUtil.Random.Next(inputParamCount)));

                //                             gene.add(newOp);
                //                             invalidOptions = null;

                //                         }

                //                         break;
                //                     }

                //                 // Apply a function
                //                 case 4:
                //                     {
                //                         // // Reduce the pollution of functions...
                //                         // if(RandomUtil.Random.Next(4)!=1)
                //                         // {
                //                         // 	break;
                //                         // }
                //                         //
                //                         // Reduce the pollution of functions...
                //                         if (Operators.Available.Functions.indexOf(gene.Operator) != -1 && RandomUtil.Random.Next(4) != 1)
                //                         {
                //                             break;
                //                         }

                //                         var newFn = Operators.GetRandomFunctionGene();

                //                         //
                //                         // // Reduce the pollution of functions...
                //                         // if(newFn.operator==og.operator)
                //                         // {
                //                         // 	if(RandomUtil.Random.Next(7)!=1)
                //                         // 	{
                //                         // 		invalidOptions.push(5);
                //                         // 		break;
                //                         // 	}
                //                         // }

                //                         if (isRoot)
                //                             newGenome.root = newFn;
                //                         else
                //                             parent.replace(gene, newFn);

                //                         newFn.add(gene);
                //                         invalidOptions = null;
                //                         break;
                //                     }
                //                 case 5:
                //                     {
                //                         if (gene.reduce())
                //                             invalidOptions = null;
                //                         break;
                //                     }
                //                 // Remove it!
                //                 default:
                //                     {

                //                         if (Operators.Available.Functions.indexOf(gene.Operator) != -1)
                //                         {
                //                             if (isRoot)
                //                             {
                //                                 if (gene.Count)
                //                                     newGenome.root = gene.linq.first();
                //                                 else
                //                                 {
                //                                     doNotRemove = true;
                //                                     break;
                //                                 }

                //                             }
                //                             else
                //                             {

                //                                 parentOp.modifyChildren(v =>
                //                                 {
                //                                     var index = v.IndexOf(gene);
                //                                     if (index != -1)
                //                                     {
                //                                         foreach (var o in gene.Reverse())
                //                                         {
                //                                             v.insert(index, o);
                //                                         }
                //                                         v.remove(gene);
                //                                         invalidOptions = null;
                //                                         return true;
                //                                     }
                //                                     return false;
                //                                 });

                //                             }
                //                             break;
                //                         }

                //                         // Just like above, consider reduction instead of trimming...
                //                         if (isRoot && gene.Count > 2)
                //                         {
                //                             gene.removeAt(RandomUtil.Random.Next(gene.Count));
                //                         }
                //                         else if (gene.Count == 2
                //                             && gene.linq.any(o => o is OperatorGene)
                //                             && gene.linq.any(o => o is ParameterGene))
                //                         {
                //                             var childOpGene = gene.linq.ofType(OperatorGene).single();
                //                             gene.remove(childOpGene);
                //                             if (isRoot)
                //                                 newGenome.root = childOpGene;
                //                             else
                //                                 parentOp.Replace(gene, childOpGene);
                //                         }
                //                         else if (shouldNotRemove() || gene.Count > 2)
                //                         {
                //                             doNotRemove = true;
                //                             break;
                //                         }
                //                         else
                //                             parentOp.Remove(gene);

                //                         invalidOptions = null;
                //                         break;

                //                     }
                //             }
                //         }
                //     }

                // }

                // newGenome.resetHash();
                return newGenome;//.setAsReadOnly();

            });

        }

    }
}