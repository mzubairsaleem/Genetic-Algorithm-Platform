var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports", "../source/Genome", "../source/GenomeFactoryBase", "../node_modules/typescript-dotnet/source/System.Linq/Linq", "../node_modules/typescript-dotnet/source/System/Integer", "./../source/Environment"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Genome_1 = require("../source/Genome");
    var GenomeFactoryBase_1 = require("../source/GenomeFactoryBase");
    var Linq_1 = require("../node_modules/typescript-dotnet/source/System.Linq/Linq");
    var Integer_1 = require("../node_modules/typescript-dotnet/source/System/Integer");
    var Environment_1 = require("./../source/Environment");
    var AlgebraGenomeFactory = (function (_super) {
        __extends(AlgebraGenomeFactory, _super);
        function AlgebraGenomeFactory() {
            _super.apply(this, arguments);
        }
        AlgebraGenomeFactory.prototype.generateSimple = function () {
            var result = new Genome_1.default();
            var op = new OperatorGene();
            op.Operator = OperatorGene.GetRandomOperator();
            result.Root = op;
            for (var i = 0, len = this._inputParamCount; i < len; i++) {
                op.Add(new ParameterGene(i));
            }
            return result;
        };
        AlgebraGenomeFactory.prototype.generate = function () {
            var _ = this, p = _._previousGenomes, ep = Linq_1.default.from(p);
            var tries = 0;
            var genome = this.generateSimple();
            var gs;
            gs = genome.hash;
            while (ep.any(function (g) { return g.hash == gs; }) && ++tries < 1000) {
                genome = _.mutate(p.getValueAt(Integer_1.default.random.under(p.count)));
                gs = genome.hash;
            }
            if (tries >= 1000)
                return null;
            this._previousGenomes.add(genome);
            return genome;
        };
        AlgebraGenomeFactory.prototype.generateFrom = function (source) {
            var sourceGenomes = Linq_1.default
                .from(source)
                .orderBy(function (s) { return s.fitness.score; })
                .select(function (s) { return s.genome; }).toArray();
            var selectionList = [];
            for (var i = 0; i < sourceGenomes.length; i++) {
                var g = sourceGenomes[i];
                for (var n = 0; n < i + 1; n++) {
                    selectionList.push(g);
                }
            }
            var tries = 0;
            var genome;
            var gs;
            do {
                genome = Mutate(selectionList[Environment_1.default.Randomizer.Next(selectionList.length)]);
                gs = genome.CachedToStringReduced;
            } while (_previousGenomes.Any(function (g) { return g.CachedToStringReduced == gs; }) && ++tries < 1000);
            if (tries >= 1000)
                return null;
            _previousGenomes.Add(genome);
            return genome;
        };
        AlgebraGenomeFactory.prototype.mutate = function (source, mutations) {
            if (mutations === void 0) { mutations = 1; }
            var newGenome = source.clone();
            for (var i = 0; i < mutations; i++) {
                var genes = newGenome.Genes.ToArray();
                var g = Environment_1.default.Randomizer.Next(genes.Length);
                var gene = genes[g];
                var isRoot = gene == newGenome.root;
                var parent = newGenome.FindParent(gene);
                var parentOp = parent;
                var invalidOptions = new List();
                var shouldNotRemove = function () { return isRoot || parent == null || parentOp == null; };
                var doNotRemove = gene instanceof ParameterGene && shouldNotRemove();
                var lastOption = -1;
                while (invalidOptions != null) {
                    if (parent != null && !parent.Children.Contains(gene))
                        throw new Exception("Parent changed?");
                    if (gene instanceof ConstantGene) {
                        var cg = gene;
                        switch (lastOption = Environment_1.default.NextRandomIntegerExcluding(2, invalidOptions)) {
                            case 0:
                                var abs = Math.Abs(cg.Multiple);
                                if (abs > 1 && Environment_1.default.Randomizer.Next(2) == 0) {
                                    if (abs != Math.Floor(abs) || Environment_1.default.Randomizer.Next(2) == 0)
                                        cg.Multiple /= abs;
                                    else
                                        cg.Multiple -= (cg.Multiple / abs);
                                }
                                else
                                    cg.Multiple *= -1;
                                invalidOptions = null;
                                break;
                            default:
                                if (parentOp != null) {
                                    parentOp.Remove(gene);
                                    invalidOptions = null;
                                }
                                break;
                        }
                    }
                    else if (gene instanceof ParameterGene) {
                        var pg = gene;
                        switch (lastOption = Environment_1.default.NextRandomIntegerExcluding(doNotRemove
                            ? 4
                            : 8, invalidOptions)) {
                            case 0:
                                var abs = Math.Abs(pg.Multiple);
                                if (abs > 1 && Environment_1.default.Randomizer.Next(2) == 0)
                                    pg.Multiple /= abs;
                                else
                                    pg.Multiple *= -1;
                                invalidOptions = null;
                                break;
                            case 1:
                                var nextParameter = Environment_1.default.NextRandomIntegerExcluding(InputParamCount, pg.ID);
                                var newPG = new ParameterGene(nextParameter);
                                if (isRoot)
                                    newGenome.Root = newPG;
                                else
                                    parent.Replace(gene, newPG);
                                invalidOptions = null;
                                break;
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
                            case 3:
                                {
                                    if (Environment_1.default.Randomizer.Next(0, 3) != 0) {
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
                            default:
                                var children = parentOp.Children;
                                if (parentOp.Count < 3) {
                                    if (children.All(function (o) { return o instanceof ParameterGene || o instanceof ConstantGene; }))
                                        doNotRemove = true;
                                    else {
                                        var replacement = children.Where(function (o) { return o instanceof OperatorGene; }).Single();
                                        if (parentOp == newGenome.Root)
                                            newGenome.Root = replacement;
                                        else
                                            newGenome
                                                .FindParent(parentOp)
                                                .Replace(parentOp, replacement);
                                    }
                                }
                                if (!doNotRemove) {
                                    parentOp.ModifyValues(function (v) { return v.Remove(gene); });
                                    invalidOptions = null;
                                }
                                break;
                        }
                    }
                    else if (gene instanceof OperatorGene) {
                        var og = gene;
                        if (OperatorGene.AvailableFunctions.Contains(og.Operator)) {
                            invalidOptions.Add(3);
                            invalidOptions.Add(4);
                        }
                        switch (lastOption = Environment_1.default.NextRandomIntegerExcluding(doNotRemove
                            ? 6
                            : 10, invalidOptions)) {
                            case 0:
                                og.Multiple *= -1;
                                invalidOptions = null;
                                break;
                            case 1:
                                var currentOperatorIndex = OperatorGene.AvailableOperators.ToList().IndexOf(og.Operator);
                                if (currentOperatorIndex == -1) {
                                    currentOperatorIndex
                                        = OperatorGene.AvailableFunctions.ToList().IndexOf(og.Operator);
                                    if (currentOperatorIndex != -1) {
                                        if (OperatorGene.AvailableFunctions.Length == 1) {
                                            invalidOptions.Add(1);
                                            break;
                                        }
                                        og.Operator = OperatorGene.AvailableFunctions[Environment_1.default.NextRandomIntegerExcluding(OperatorGene.AvailableFunctions.Length, currentOperatorIndex)];
                                    }
                                    break;
                                }
                                var newOperatorIndex = Environment_1.default.NextRandomIntegerExcluding(OperatorGene.AvailableOperators.Length, currentOperatorIndex);
                                if (og.Count > 2 && Environment_1.default.Randomizer.Next(og.Count) != 0) {
                                    var startIndex = Environment_1.default.Randomizer.Next(og.Count - 1);
                                    var endIndex = startIndex == 0
                                        ? Environment_1.default.Randomizer.Next(1, og.Count - 1)
                                        : Environment_1.default.Randomizer.Next(startIndex + 1, og.Count);
                                    og.ModifyValues(function (v) {
                                        var contents = v.GetRange(startIndex, endIndex - startIndex);
                                        for (var _i = 0, contents_1 = contents; _i < contents_1.length; _i++) {
                                            var o = contents_1[_i];
                                            v.Remove(o);
                                        }
                                        var O = OperatorGene.GetRandomOperation();
                                        O.AddRange(contents);
                                        v.Insert(startIndex, O);
                                    });
                                }
                                else
                                    og.Operator = OperatorGene.AvailableOperators[newOperatorIndex];
                                invalidOptions = null;
                                break;
                            case 2:
                                if (og.Operator == '/' && og.Count > 1)
                                    break;
                                og.Add(new ParameterGene(Environment_1.default.Randomizer.Next(InputParamCount)));
                                invalidOptions = null;
                                break;
                            case 3:
                                var first = new ParameterGene(Environment_1.default.Randomizer.Next(InputParamCount));
                                var newOp = InputParamCount == 1
                                    ? OperatorGene.GetRandomOperation('/')
                                    : OperatorGene.GetRandomOperation();
                                newOp.Add(first);
                                if (newOp.Operator == '/')
                                    newOp.Add(new ParameterGene(Environment_1.default.NextRandomIntegerExcluding(InputParamCount, first.ID)));
                                else
                                    newOp.Add(new ParameterGene(Environment_1.default.Randomizer.Next(InputParamCount)));
                                og.Add(newOp);
                                invalidOptions = null;
                                break;
                            case 4:
                                {
                                    if (Environment_1.default.Randomizer.Next(4) != 1) {
                                        break;
                                    }
                                    if (OperatorGene.AvailableFunctions.Contains(og.Operator) && Environment_1.default.Randomizer.Next(4) != 1) {
                                        break;
                                    }
                                    var newFn = OperatorGene.GetRandomFunction();
                                    if (newFn.Operator == og.Operator) {
                                        if (Environment_1.default.Randomizer.Next(7) != 1) {
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
                                if (og.Reduce())
                                    invalidOptions = null;
                                break;
                            default:
                                if (OperatorGene.AvailableFunctions.Contains(og.Operator)) {
                                    if (isRoot) {
                                        if (og.Children.Any())
                                            newGenome.Root = og.Children.First();
                                        else {
                                            doNotRemove = true;
                                            break;
                                        }
                                    }
                                    else {
                                        parentOp.ModifyValues(function (v) {
                                            var index = v.IndexOf(gene);
                                            if (index != -1) {
                                                v.InsertRange(index, og.Children);
                                                v.Remove(gene);
                                                invalidOptions = null;
                                            }
                                        });
                                    }
                                    break;
                                }
                                if (isRoot && og.Count > 2) {
                                    og.ModifyValues(function (v) { return v.RemoveAt(Environment_1.default.Randomizer.Next(v.Count)); });
                                }
                                else if (og.Count == 2 && og.Children.Any(function (o) { return o instanceof OperatorGene; }) && og.Children.Any(function (o) { return o instanceof ParameterGene; })) {
                                    var childOpGene = og.Children.Single(function (o) { return o instanceof OperatorGene; });
                                    og.Remove(childOpGene);
                                    if (isRoot)
                                        newGenome.Root = childOpGene;
                                    else
                                        parentOp.Replace(og, childOpGene);
                                }
                                else if (shouldNotRemove() || og.Count < 3) {
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
        };
        return AlgebraGenomeFactory;
    }(GenomeFactoryBase_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraGenomeFactory;
});
//# sourceMappingURL=AlgebraGenomeFactory.js.map