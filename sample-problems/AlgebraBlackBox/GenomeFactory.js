"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var GenomeFactoryBase_1 = require("../../source/GenomeFactoryBase");
var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
var Genome_1 = require("./Genome");
var Operator_1 = require("./Genes/Operator");
var ParameterGene_1 = require("./Genes/ParameterGene");
var Operator = require("./Operators");
var ConstantGene_1 = require("./Genes/ConstantGene");
var nextRandomIntegerExcluding_1 = require("../../source/nextRandomIntegerExcluding");
var Random_1 = require("typescript-dotnet-umd/System/Random");
var Types_1 = require("typescript-dotnet-umd/System/Types");
var AlgebraGenomeFactory = (function (_super) {
    __extends(AlgebraGenomeFactory, _super);
    function AlgebraGenomeFactory() {
        return _super.apply(this, arguments) || this;
    }
    AlgebraGenomeFactory.prototype.generateParamOnly = function (id) {
        return new Genome_1.default(new ParameterGene_1.default(id));
    };
    AlgebraGenomeFactory.prototype.generateOperated = function (paramCount) {
        if (paramCount === void 0) { paramCount = 1; }
        var op = Operator_1.default.getRandomOperation();
        var result = new Genome_1.default(op);
        for (var i = 0; i < paramCount; i++) {
            op.add(new ParameterGene_1.default(i));
        }
        return result;
    };
    AlgebraGenomeFactory.prototype.generate = function (source) {
        var _ = this, p = _._previousGenomes;
        var attempts = 0;
        var genome = null;
        var hash = null;
        if (source && source.length) {
            for (var m = 1; m < 4; m++) {
                var tries = 200;
                do {
                    genome = _.mutate(Random_1.Random.select.one(source, true), m);
                    hash = genome.hash;
                    attempts++;
                } while (p.containsKey(hash) && --tries);
                if (tries)
                    break;
                else
                    genome = null;
            }
        }
        if (!genome) {
            for (var m = 1; m < 4; m++) {
                var tries = 10, paramCount = 0;
                do {
                    {
                        genome = this.generateParamOnly(paramCount);
                        hash = genome.hash;
                        attempts++;
                        if (!p.containsKey(hash))
                            break;
                    }
                    paramCount += 1;
                    {
                        genome = this.generateOperated(paramCount + 1);
                        hash = genome.hash;
                        attempts++;
                        if (!p.containsKey(hash))
                            break;
                    }
                    var t = Math.min(p.count * 2, 100);
                    do {
                        genome = _.mutate(p.getValueByIndex(Random_1.Random.integer(p.count)), m);
                        hash = genome.hash;
                        attempts++;
                    } while (p.containsKey(hash) && --t);
                    if (t)
                        break;
                } while (--tries);
                if (tries)
                    break;
                else
                    genome = null;
            }
        }
        if (genome && hash)
            p.addByKeyValue(hash, genome);
        return genome;
    };
    AlgebraGenomeFactory.prototype.mutate = function (source, mutations) {
        if (mutations === void 0) { mutations = 1; }
        var inputParamCount = source.genes.ofType(ParameterGene_1.default).count();
        var newGenome = source.clone();
        var _loop_1 = function (i) {
            var genes = newGenome.genes.toArray();
            var gene = Random_1.Random.select.one(genes, true);
            var isRoot = gene == newGenome.root;
            var parent_1 = newGenome.findParent(gene);
            var parentOp = Types_1.Type.as(parent_1, Operator_1.default);
            var invalidOptions = [];
            var shouldNotRemove = function () { return isRoot || parent_1 == null || parentOp == null; };
            var doNotRemove = gene instanceof ParameterGene_1.default && shouldNotRemove();
            var lastOption = -1;
            var _loop_2 = function () {
                if (parent_1 != null && !parent_1.contains(gene))
                    throw "Parent changed?";
                if (gene instanceof ConstantGene_1.default) {
                    var cg = gene;
                    switch (lastOption = nextRandomIntegerExcluding_1.default(2, invalidOptions)) {
                        case 0:
                            var abs = Math.abs(cg.multiple);
                            if (abs > 1 && Random_1.Random.next(2) == 0) {
                                if (abs != Math.floor(abs) || Random_1.Random.next(2) == 0)
                                    cg.multiple /= abs;
                                else
                                    cg.multiple -= (cg.multiple / abs);
                            }
                            else
                                cg.multiple *= -1;
                            invalidOptions = null;
                            break;
                        default:
                            if (parentOp != null) {
                                parentOp.remove(gene);
                                invalidOptions = null;
                            }
                            break;
                    }
                }
                else if (gene instanceof ParameterGene_1.default) {
                    var pg = gene;
                    switch (lastOption = nextRandomIntegerExcluding_1.default(doNotRemove
                        ? 4
                        : 6, invalidOptions)) {
                        case 0:
                            {
                                var abs = Math.abs(pg.multiple);
                                if (abs > 1 && Random_1.Random.next(2) == 0)
                                    pg.multiple /= abs;
                                else
                                    pg.multiple *= -1;
                                invalidOptions = null;
                                break;
                            }
                        case 1:
                            var nextParameter = nextRandomIntegerExcluding_1.default(inputParamCount + 1, pg.id);
                            var newPG = new ParameterGene_1.default(nextParameter);
                            if (isRoot)
                                newGenome.root = newPG;
                            else
                                parent_1.replace(gene, newPG);
                            invalidOptions = null;
                            break;
                        case 2:
                            {
                                var newFn = Operator_1.default.getRandomOperation(Operator.DIVIDE);
                                if (isRoot)
                                    newGenome.root = newFn;
                                else
                                    parent_1.replace(gene, newFn);
                                newFn.add(gene);
                                newFn.add(gene.clone());
                                invalidOptions = null;
                                break;
                            }
                        case 3:
                            {
                                if (Random_1.Random.next.inRange(0, 3) != 0) {
                                    invalidOptions.push(4);
                                    break;
                                }
                                var newFn = new Operator_1.default(Operator_1.default.getRandomFunctionOperator());
                                if (isRoot)
                                    newGenome.root = newFn;
                                else
                                    parent_1.replace(gene, newFn);
                                newFn.add(gene);
                                invalidOptions = null;
                                break;
                            }
                        default:
                            if (!parentOp)
                                throw "Missing parent operator";
                            if (parentOp.count < 3) {
                                if (parentOp.linq.all(function (o) { return o instanceof ParameterGene_1.default || o instanceof ConstantGene_1.default; }))
                                    doNotRemove = true;
                                else {
                                    var replacement = parentOp.linq
                                        .where(function (o) { return o instanceof Operator_1.default; })
                                        .single();
                                    if (parentOp == newGenome.root)
                                        newGenome.root = replacement;
                                    else
                                        newGenome
                                            .findParent(parentOp)
                                            .replace(parentOp, replacement);
                                }
                            }
                            if (!doNotRemove) {
                                parentOp.remove(gene);
                                invalidOptions = null;
                            }
                            break;
                    }
                }
                else if (gene instanceof Operator_1.default) {
                    var og_1 = gene;
                    if (Operator.Available.Functions.indexOf(og_1.operator) != -1) {
                        invalidOptions.push(3);
                        invalidOptions.push(4);
                    }
                    switch (lastOption = nextRandomIntegerExcluding_1.default(doNotRemove
                        ? 6
                        : 10, invalidOptions)) {
                        case 0:
                            og_1.multiple *= -1;
                            invalidOptions = null;
                            break;
                        case 1:
                            var currentOperatorIndex = Operator.Available.Operators.indexOf(og_1.operator);
                            if (currentOperatorIndex == -1) {
                                currentOperatorIndex
                                    = Operator.Available.Functions.indexOf(og_1.operator);
                                if (currentOperatorIndex != -1) {
                                    if (Operator.Available.Functions.length == 1) {
                                        invalidOptions.push(1);
                                        break;
                                    }
                                    og_1.operator = Operator.Available.Functions[nextRandomIntegerExcluding_1.default(Operator.Available.Functions.length, currentOperatorIndex)];
                                }
                                break;
                            }
                            var newOperatorIndex = nextRandomIntegerExcluding_1.default(Operator.Available.Operators.length, currentOperatorIndex);
                            if (og_1.count > 2 && Random_1.Random.next(og_1.count) != 0) {
                                var startIndex_1 = Random_1.Random.next(og_1.count - 1);
                                var endIndex_1 = startIndex_1 == 0
                                    ? Random_1.Random.next.inRange(1, og_1.count - 1)
                                    : Random_1.Random.next.inRange(startIndex_1 + 1, og_1.count);
                                og_1.modifyChildren(function (v) {
                                    var contents = Linq_1.default(v)
                                        .skip(startIndex_1)
                                        .take(endIndex_1 - startIndex_1)
                                        .toArray();
                                    for (var _i = 0, contents_1 = contents; _i < contents_1.length; _i++) {
                                        var o = contents_1[_i];
                                        v.remove(o);
                                    }
                                    var O = Operator_1.default.getRandomOperation();
                                    O.importEntries(contents);
                                    v.insert(startIndex_1, O);
                                    return true;
                                });
                            }
                            else
                                og_1.operator = Operator.Available.Operators[newOperatorIndex];
                            invalidOptions = null;
                            break;
                        case 2:
                            if (og_1.operator == Operator.DIVIDE && og_1.count > 1)
                                break;
                            og_1.add(new ParameterGene_1.default(Random_1.Random.next(inputParamCount)));
                            invalidOptions = null;
                            break;
                        case 3:
                            var first = new ParameterGene_1.default(Random_1.Random.next(inputParamCount));
                            var newOp = inputParamCount <= 1
                                ? Operator_1.default.getRandomOperation('/')
                                : Operator_1.default.getRandomOperation();
                            newOp.add(first);
                            if (newOp.operator == Operator.DIVIDE)
                                newOp.add(new ParameterGene_1.default(nextRandomIntegerExcluding_1.default(inputParamCount, first.id)));
                            else
                                newOp.add(new ParameterGene_1.default(Random_1.Random.next(inputParamCount)));
                            og_1.add(newOp);
                            invalidOptions = null;
                            break;
                        case 4:
                            {
                                if (Operator.Available.Functions.indexOf(og_1.operator) != -1 && Random_1.Random.next(4) != 1) {
                                    break;
                                }
                                var newFn = new Operator_1.default(Operator_1.default.getRandomFunctionOperator());
                                if (isRoot)
                                    newGenome.root = newFn;
                                else
                                    parent_1.replace(gene, newFn);
                                newFn.add(gene);
                                invalidOptions = null;
                                break;
                            }
                        case 5:
                            if (og_1.reduce())
                                invalidOptions = null;
                            break;
                        default:
                            if (Operator.Available.Functions.indexOf(og_1.operator) != -1) {
                                if (isRoot) {
                                    if (og_1.count)
                                        newGenome.root = og_1.linq.first();
                                    else {
                                        doNotRemove = true;
                                        break;
                                    }
                                }
                                else {
                                    parentOp.modifyChildren(function (v) {
                                        var index = v.indexOf(og_1);
                                        if (index != -1) {
                                            for (var _i = 0, _a = og_1.toArray().reverse(); _i < _a.length; _i++) {
                                                var o = _a[_i];
                                                v.insert(index, o);
                                            }
                                            v.remove(og_1);
                                            invalidOptions = null;
                                            return true;
                                        }
                                        return false;
                                    });
                                }
                                break;
                            }
                            if (isRoot && og_1.count > 2) {
                                og_1.removeAt(Random_1.Random.next(og_1.count));
                            }
                            else if (og_1.count == 2
                                && og_1.linq.any(function (o) { return o instanceof Operator_1.default; })
                                && og_1.linq.any(function (o) { return o instanceof ParameterGene_1.default; })) {
                                var childOpGene = og_1.linq.ofType(Operator_1.default).single();
                                og_1.remove(childOpGene);
                                if (isRoot)
                                    newGenome.root = childOpGene;
                                else
                                    parentOp.replace(og_1, childOpGene);
                            }
                            else if (shouldNotRemove() || og_1.count > 2) {
                                doNotRemove = true;
                                break;
                            }
                            else
                                parentOp.remove(gene);
                            invalidOptions = null;
                            break;
                    }
                }
            };
            while (invalidOptions != null) {
                _loop_2();
            }
        };
        for (var i = 0; i < mutations; i++) {
            _loop_1(i);
        }
        newGenome.resetHash();
        return newGenome;
    };
    return AlgebraGenomeFactory;
}(GenomeFactoryBase_1.default));
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = AlgebraGenomeFactory;
//# sourceMappingURL=GenomeFactory.js.map