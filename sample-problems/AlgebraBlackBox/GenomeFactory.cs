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


        public static Genome ApplyClone(Genome source, int geneIndex, Action<IGene, Genome> handler)
        {
            if (geneIndex == -1)
                throw new ArgumentOutOfRangeException("geneIndex", "Can't be -1.");
            var newGenome = source.Clone();
            var gene = newGenome.Genes.ElementAt(geneIndex);
            handler(gene, newGenome);
            return newGenome.AsReduced();
        }

        public static Genome ApplyClone(Genome source, int geneIndex, Action<IGene> handler)
        {
            if (geneIndex == -1)
                throw new ArgumentOutOfRangeException("geneIndex", "Can't be -1.");
            var newGenome = source.Clone();
            var gene = newGenome.Genes.ElementAt(geneIndex);
            handler(gene);
            return newGenome;
        }

        public static Genome ApplyClone(Genome source, IGene gene, Action<IGene, Genome> handler)
        {
            return ApplyClone(source, source.Genes.Memoize().IndexOf(gene), handler);
        }

        public static Genome ApplyClone(Genome source, IGene gene, Action<IGene> handler)
        {
            return ApplyClone(source, source.Genes.Memoize().IndexOf(gene), handler);
        }

        public static class VariationCatalog
        {

            public static Genome ReduceMultipleMagnitude(Genome source, int geneIndex)
            {
                return ApplyClone(source, geneIndex, (gene) =>
                {
                    var absMultiple = Math.Abs(gene.Multiple);
                    gene.Multiple -= gene.Multiple / absMultiple;
                });
            }

            public static bool CheckRemovalValidity(Genome source, IGene gene)
            {
                // Validate worthyness.
                var parent = source.FindParent(gene);

                // Search for potential futility...
                // Basically, if there is no dynamic genes left after reduction then it's not worth removing.
                if (parent != source.Root && parent.Count(g => g != gene && !(g is ConstantGene)) == 0)
                {
                    return CheckRemovalValidity(source, parent);
                }

                return false;
            }

            public static Genome RemoveGene(Genome source, int geneIndex)
            {
                // Validate worthyness.
                var gene = source.Genes.ElementAt(geneIndex);

                if (CheckRemovalValidity(source, gene))
                {
                    return ApplyClone(source, geneIndex, (g, newGenome) =>
                    {
                        var parent = source.FindParent(g);
                        parent.Remove(g);
                    });
                }
                return null;

            }
            public static Genome RemoveGene(Genome source, IGene gene)
            {
                return RemoveGene(source, source.Genes.Memoize().IndexOf(gene));
            }

            public static bool CheckPromoteChildrenValidity(Genome source, IGene gene)
            {
                // Validate worthyness.
                var op = gene as GeneNode;
                return op != null && op.Count == 1;
            }

            public static Genome PromoteChildren(Genome source, int geneIndex)
            {
                // Validate worthyness.
                var gene = source.Genes.ElementAt(geneIndex);

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
                return ApplyFunction(source, source.Genes.Memoize().IndexOf(gene), fn);
            }
        }

        public static class MutationCatalog
        {
            public static Genome MutateSign(Genome source, IGene gene)
            {
                return ApplyClone(source, gene, g =>
                {
                    switch (RandomUtil.Random.Next(3))
                    {
                        case 0:
                            // Alter Sign
                            gene.Multiple *= -1;
                            break;
                        case 1:
                            // Increase multiple.
                            gene.Multiple += 1;
                            break;
                        case 2:
                            // Decrease multiple.
                            gene.Multiple -= 1;
                            break;
                    }
                });
            }

            public static Genome MutateParameter(Genome source, ParameterGene gene)
            {
                return ApplyClone(source, gene, (g, newGenome) =>
                {
                    var parameter = (ParameterGene)g;
                    var inputParamCount = newGenome.Genes.OfType<ParameterGene>().Count();
                    var nextParameter = RandomUtil.NextRandomIntegerExcluding(inputParamCount + 1, parameter.ID);
                    newGenome.Replace(g, new ParameterGene(nextParameter, parameter.Multiple));
                });
            }

            public static Genome Split(Genome source, IGene gene)
            {
                return ApplyClone(source, gene, (g, newGenome) =>
                {
                    var newFn = Operators.GetRandomOperationGene(Operators.DIVIDE); // excluding divide
                    newGenome.Replace(g, newFn);
                    newFn.Add(gene);
                    newFn.Add(gene.Clone());
                });
            }

        }

        public override Task<IEnumerable<Genome>> GenerateVariations(Genome source)
        {
            return new Task<IEnumerable<Genome>>(() =>
            {
                var result = new List<Genome>();
                var sourceGenes = source.Genes.ToArray();
                var count = sourceGenes.Length;
                for (var i = 0; i < count; i++)
                {
                    var gene = sourceGenes[i];
                    var isRoot = gene == source.Root;

                    result.Add(VariationCatalog.ReduceMultipleMagnitude(source, i));
                    result.Add(VariationCatalog.RemoveGene(source, i));
                    result.Add(VariationCatalog.PromoteChildren(source, i));

                    foreach (var fn in Operators.Available.Functions)
                    {
                        result.Add(VariationCatalog.ApplyFunction(source, i, fn));
                    }
                }

                return result
                    .Where(genome => genome != null)
                    .Select(genome =>
                    {
                        Genome temp;
                        return _previousGenomes.TryGetValue(genome.Hash, out temp) ? temp : genome;
                    });
            });
        }

        public Task<Genome> Mutate(Genome source)
        {
            return new Task<Genome>(() =>
            {
                /* Possible mutations:
                 * 1) Adding a parameter node to an operation.
                 * 2) Apply a function to node.
                 * 3) Adding an operator and a parameter node.
                 * 4) Removing a node from an operation.
                 * 5) Removing an operation.
                 * 6) Removing a function. */

                var genes = source.Genes.ToList();

                while (genes.Any())
                {
                    var gene = genes.RandomSelectOne();
                    if (gene is ConstantGene)
                    {
                        return MutationCatalog.MutateSign(source, gene);
                    }
                    else if (gene is ParameterGene)
                    {

                        var options = Enumerable.Range(1, 4).ToList();
                        while (options.Any())
                        {
                            switch (options.RandomPluck())
                            {
                                case 0:
                                    return MutationCatalog
                                    .MutateSign(source, gene);

                                // Simply change parameters
                                case 1:
                                    return MutationCatalog
                                    .MutateParameter(source, (ParameterGene)gene);

                                // Split it...
                                case 2:
                                    return MutationCatalog
                                    .Split(source, gene);

                                // Apply a function
                                case 3:
                                    // Reduce the pollution of functions...
                                    if (RandomUtil.Random.Next(0, 4) == 0)
                                    {
                                        return VariationCatalog
                                        .ApplyFunction(source, gene, Operators.GetRandomFunction());
                                    }
                                    break;

                                // Remove it!
                                case 4:
                                    var attempt = VariationCatalog.RemoveGene(source, gene);
                                    if (attempt != null)
                                        return attempt;
                                    break;


                            }
                        }


                    }
                    else if (gene is OperatorGeneBase)
                    {
                        var og = gene as OperatorGeneBase;
                        var isFn = gene is FunctionGene;
                        if (isFn)
                        {
                            if (!isRoot) // Might need to break it out.
                                invalidOptions.Add(3);
                            invalidOptions.Add(4);
                        }

                        switch (lastOption = RandomUtil.NextRandomIntegerExcluding(doNotRemove
                            ? 6
                            : 10, invalidOptions))
                        {
                            // Simply invert the sign
                            case 0:
                                {
                                    gene.Multiple *= -1;
                                    invalidOptions = null;
                                    break;
                                }

                            // Simply change operations
                            case 1:
                                {
                                    var currentOperatorIndex
                                            = Operators.Available.Operators.IndexOf(og.Operator);
                                    if (currentOperatorIndex == -1)
                                    {
                                        currentOperatorIndex
                                            = Operators.Available.Functions.IndexOf(og.Operator);
                                        if (currentOperatorIndex != -1)
                                        {
                                            if (Operators.Available.Functions.Count == 1)
                                            {
                                                invalidOptions.Add(1);
                                                break;
                                            }

                                            gene.Operator = Operators.Available.Functions[
                                                RandomUtil.NextRandomIntegerExcluding(Operators.Available.Functions.Count, currentOperatorIndex)];
                                        }


                                        break;
                                    }

                                    var newOperatorIndex = RandomUtil.NextRandomIntegerExcluding(Operators.Available.Operators.Count, currentOperatorIndex);

                                    // Decide if we will also change the grouping.
                                    if (gene.Count > 2 && RandomUtil.Random.Next(gene.Count) != 0)
                                    {
                                        var startIndex = RandomUtil.Random.Next(gene.Count - 1);
                                        var endIndex = startIndex == 0
                                            ? RandomUtil.Random.Next.inRange(1, gene.Count - 1)
                                            : RandomUtil.Random.Next.inRange(startIndex + 1, gene.Count);

                                        gene.Sync.Modifying(() =>
                                        {
                                            var contents = gene
                                                .Skip(startIndex)
                                                .Take(endIndex - startIndex)
                                                .ToArray();

                                            foreach (var o in contents)
                                            {
                                                gene.Remove(o);
                                            }

                                            var O = OperatorGene.GetRandomOperationGene();
                                            O.Add(contents);
                                            gene.Insert(startIndex, O);

                                            return true;
                                        });
                                    }
                                    else // Grouping remains... Only operator changes.
                                        gene.Operator = Operators.Available.Operators[newOperatorIndex];

                                    invalidOptions = null;
                                    break;
                                }

                            // Add random parameter.
                            case 2:
                                {
                                    if (gene.Operator == Operator.SQUARE_ROOT
                                        // In order to avoid unnecessary reduction, avoid adding subsequent divisors.
                                        || gene.Operator == Operator.DIVIDE && gene.Count > 1)
                                        break;

                                    gene.add(new ParameterGene(RandomUtil.Random.Next(inputParamCount)));
                                    invalidOptions = null;
                                    break;
                                }

                            // Add random operator branch.
                            case 3:
                                {
                                    var n = new ParameterGene(RandomUtil.Random.Next(inputParamCount));

                                    var newOp = inputParamCount <= 1
                                        ? OperatorGene.getRandomOperation('/')
                                        : OperatorGene.getRandomOperation();

                                    if (isFn || RandomUtil.Random.Next(4) == 0)
                                    {
                                        // logic states that this MUST be the root node.
                                        var index = RandomUtil.Random.Next(2);
                                        if (index)
                                        {
                                            newOp.add(n);
                                            newOp.add(newGenome.root);
                                        }
                                        else
                                        {
                                            newOp.add(newGenome.root);
                                            newOp.add(n);
                                        }

                                        newGenome.root = newOp;
                                    }
                                    else
                                    {

                                        newOp.add(n);

                                        // Useless to divide a param by itself, avoid...
                                        if (newOp.Operator == Operators.DIVIDE)
                                            newOp.Add(new ParameterGene(RandomUtil.NextRandomIntegerExcluding(inputParamCount, n.id)));
                                        else
                                            newOp.Add(new ParameterGene(RandomUtil.Random.Next(inputParamCount)));

                                        gene.add(newOp);
                                        invalidOptions = null;

                                    }

                                    break;
                                }

                            // Apply a function
                            case 4:
                                {
                                    // // Reduce the pollution of functions...
                                    // if(RandomUtil.Random.Next(4)!=1)
                                    // {
                                    // 	break;
                                    // }
                                    //
                                    // Reduce the pollution of functions...
                                    if (Operators.Available.Functions.indexOf(gene.Operator) != -1 && RandomUtil.Random.Next(4) != 1)
                                    {
                                        break;
                                    }

                                    var newFn = Operators.GetRandomFunctionGene();

                                    //
                                    // // Reduce the pollution of functions...
                                    // if(newFn.operator==og.operator)
                                    // {
                                    // 	if(RandomUtil.Random.Next(7)!=1)
                                    // 	{
                                    // 		invalidOptions.push(5);
                                    // 		break;
                                    // 	}
                                    // }

                                    if (isRoot)
                                        newGenome.root = newFn;
                                    else
                                        parent.replace(gene, newFn);

                                    newFn.add(gene);
                                    invalidOptions = null;
                                    break;
                                }
                            case 5:
                                {
                                    if (gene.reduce())
                                        invalidOptions = null;
                                    break;
                                }
                            // Remove it!
                            default:
                                {

                                    if (Operators.Available.Functions.indexOf(gene.Operator) != -1)
                                    {
                                        if (isRoot)
                                        {
                                            if (gene.Count)
                                                newGenome.root = gene.linq.first();
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
                                                var index = v.IndexOf(gene);
                                                if (index != -1)
                                                {
                                                    foreach (var o in gene.Reverse())
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
                                    if (isRoot && gene.Count > 2)
                                    {
                                        gene.removeAt(RandomUtil.Random.Next(gene.Count));
                                    }
                                    else if (gene.Count == 2
                                        && gene.linq.any(o => o is OperatorGene)
                                        && gene.linq.any(o => o is ParameterGene))
                                    {
                                        var childOpGene = gene.linq.ofType(OperatorGene).single();
                                        gene.remove(childOpGene);
                                        if (isRoot)
                                            newGenome.root = childOpGene;
                                        else
                                            parentOp.Replace(gene, childOpGene);
                                    }
                                    else if (shouldNotRemove() || gene.Count > 2)
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

                return null;
            });
        }

        public override Task<Genome> Mutate(Genome source, uint mutations)
        {
            return new Task<Genome>(() =>
            {
                Genome genome = null;
                for (uint i = 0; i < mutations; i++)
                {
                    uint tries = 3;
                    do
                    {
                        var r = Mutate(source);
                        r.Wait();
                        genome = r.Result;
                    }
                    while (genome == null && --tries != 0);
                    // Reuse the clone as the source 
                    if (genome == null) break; // No mutation possible? :/
                    source = genome;
                }
                return genome;
            });

        }

    }
}