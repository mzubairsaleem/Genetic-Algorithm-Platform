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
            return newGenome;
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

            public static Genome PromoteChildren(Genome source, IGene gene)
            {
                return PromoteChildren(source, source.Genes.Memoize().IndexOf(gene));
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
                    if (gene is SquareRootGene)
                        return null;
                }

                return ApplyClone(source, gene, (g, newGenome) =>
                {
                    var og = (OperatorGeneBase)g;
                    var inputParamCount = newGenome.Genes.OfType<ParameterGene>().Count();
                    og.Add(new ParameterGene(RandomUtil.Random.Next(inputParamCount + 1)));
                });
            }

            public static Genome BranchOperation(Genome source, OperatorGeneBase gene)
            {
                return ApplyClone(source, gene, (g, newGenome) =>
                {
                    var inputParamCount = source.Genes.OfType<ParameterGene>().Count();
                    var n = new ParameterGene(RandomUtil.Random.Next(inputParamCount));
                    var newOp = Operators.GetRandomOperationGene();

                    if (gene is FunctionGene || RandomUtil.Random.Next(4) == 0)
                    {
                        var index = RandomUtil.Random.Next(2);
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
                        newOp.Add(new ParameterGene(RandomUtil.Random.Next(inputParamCount)));

                        ((OperatorGeneBase)g).Add(newOp);
                    }

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
                        genome = genome.AsReduced();
                        Genome temp;
                        return _previousGenomes.TryGetValue(genome.CachedToString, out temp) ? temp : genome;
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
                        switch (RandomUtil.Random.Next(3))
                        {
                            case 0:
                                return VariationCatalog
                                .ApplyFunction(source, gene, Operators.GetRandomFunction());
                            default:
                                // Apply a function
                                return MutationCatalog
                                .MutateSign(source, gene);
                        }

                    }
                    else if (gene is ParameterGene)
                    {
                        var options = Enumerable.Range(0, 4).ToList();
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
                        var options = Enumerable.Range(0, 6).ToList();
                        while (options.Any())
                        {
                            Genome ng = null;
                            switch (options.RandomPluck())
                            {
                                case 0:
                                    ng = MutationCatalog
                                    .MutateSign(source, gene);
                                    break;

                                case 1:
                                    ng = VariationCatalog
                                    .PromoteChildren(source, gene);
                                    break;

                                case 2:
                                    ng = MutationCatalog
                                    .ChangeOperation(source, (OperatorGeneBase)gene);
                                    break;

                                // Apply a function
                                case 3:
                                    // Reduce the pollution of functions...
                                    if (RandomUtil.Random.Next(0, 4) == 0)
                                    {
                                        return VariationCatalog
                                        .ApplyFunction(source, gene, Operators.GetRandomFunction());
                                    }
                                    break;

                                case 4:
                                    ng = VariationCatalog.RemoveGene(source, gene);
                                    break;

                                case 5:
                                    ng = MutationCatalog
                                    .AddParameter(source, (OperatorGeneBase)gene);
                                    break;

                                case 6:
                                    ng = MutationCatalog
                                    .BranchOperation(source, (OperatorGeneBase)gene);
                                    break;

                            }

                            if (ng != null)
                                return ng;
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