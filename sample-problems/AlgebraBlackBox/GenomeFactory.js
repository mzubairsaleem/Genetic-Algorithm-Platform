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
        define(["require", "exports", "../../source/GenomeFactoryBase", "../../node_modules/typescript-dotnet/source/System.Linq/Linq", "../../node_modules/typescript-dotnet/source/System/Integer", "./Genome", "./Genes/Operator", "./Genes/ParameterGene", "./Operators", "./Genes/ConstantGene", "../../source/nextRandomIntegerExcluding"], factory);
    }
})(function (require, exports) {
    "use strict";
    var GenomeFactoryBase_1 = require("../../source/GenomeFactoryBase");
    var Linq_1 = require("../../node_modules/typescript-dotnet/source/System.Linq/Linq");
    var Integer_1 = require("../../node_modules/typescript-dotnet/source/System/Integer");
    var Genome_1 = require("./Genome");
    var Operator_1 = require("./Genes/Operator");
    var ParameterGene_1 = require("./Genes/ParameterGene");
    var Operator = require("./Operators");
    var ConstantGene_1 = require("./Genes/ConstantGene");
    var nextRandomIntegerExcluding_1 = require("../../source/nextRandomIntegerExcluding");
    var triangular;
    (function (triangular) {
        function forward(n) {
            return n * (n + 1) / 2;
        }
        triangular.forward = forward;
        function reverse(n) {
            return (Math.sqrt(8 * n + 1) - 1) / 2 | 0;
        }
        triangular.reverse = reverse;
    })(triangular || (triangular = {}));
    var AlgebraGenomeFactory = (function (_super) {
        __extends(AlgebraGenomeFactory, _super);
        function AlgebraGenomeFactory() {
            _super.apply(this, arguments);
        }
        AlgebraGenomeFactory.prototype.generateSimple = function (paramCount) {
            if (paramCount === void 0) { paramCount = 1; }
            var result = new Genome_1.default();
            var op = Operator_1.default.getRandomOperation();
            result.root = op;
            for (var i = 0; i < paramCount; i++) {
                op.add(new ParameterGene_1.default(i));
            }
            return result;
        };
        AlgebraGenomeFactory.prototype.generate = function (paramCount) {
            if (paramCount === void 0) { paramCount = 1; }
            var _ = this, p = _._previousGenomes;
            var tries = 1000;
            var genome = this.generateSimple(paramCount);
            var hash = genome.hash;
            while (p.containsKey(hash) && --tries) {
                genome = _.mutate(p.getValueByIndex(Integer_1.default.random(p.count)));
                hash = genome.hash;
            }
            if (!tries)
                return null;
            p.addByKeyValue(hash, genome);
            return genome;
        };
        AlgebraGenomeFactory.prototype.generateFrom = function (source, rankingComparer) {
            var sourceGenomes;
            {
                var s = Linq_1.default.from(source);
                if (rankingComparer)
                    s = s.orderUsing(rankingComparer);
                sourceGenomes = s.toArray();
            }
            var count = sourceGenomes.length;
            var t = triangular.forward(count);
            var tries = 1000, p = this._previousGenomes;
            var genome;
            var hash;
            do {
                var i = triangular.reverse(Integer_1.default.random(t));
                genome = this.mutate(sourceGenomes[i]);
                hash = genome.hash;
            } while (p.containsKey(hash) && --tries);
            if (!tries)
                return null;
            p.addByKeyValue(hash, genome);
            return genome;
        };
        AlgebraGenomeFactory.prototype.mutate = function (source, mutations) {
            if (mutations === void 0) { mutations = 1; }
            var inputParamCount = source.root.descendants.ofType(ParameterGene_1.default).count();
            var newGenome = source.clone();
            var _loop_1 = function() {
                var genes = newGenome.root.descendants.toArray();
                var gene = Integer_1.default.random.select(genes);
                var isRoot = gene == newGenome.root;
                var parent_1 = newGenome.findParent(gene);
                var parentOp = parent_1 instanceof Operator_1.default ? parent_1 : null;
                var invalidOptions = [];
                var shouldNotRemove = function () { return isRoot || parent_1 == null || parentOp == null; };
                var doNotRemove = gene instanceof ParameterGene_1.default && shouldNotRemove();
                var lastOption = -1;
                var _loop_2 = function() {
                    if (parent_1 != null && !parent_1.contains(gene))
                        throw "Parent changed?";
                    if (gene instanceof ConstantGene_1.default) {
                        cg = gene;
                        switch (lastOption = nextRandomIntegerExcluding_1.default(2, invalidOptions)) {
                            case 0:
                                abs = Math.abs(cg.multiple);
                                if (abs > 1 && Integer_1.default.random.next(2) == 0) {
                                    if (abs != Math.floor(abs) || Integer_1.default.random.next(2) == 0)
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
                        pg = gene;
                        switch (lastOption = nextRandomIntegerExcluding_1.default(doNotRemove
                            ? 4
                            : 8, invalidOptions)) {
                            case 0:
                                {
                                    var abs_1 = Math.abs(pg.multiple);
                                    if (abs_1 > 1 && Integer_1.default.random.next(2) == 0)
                                        pg.multiple /= abs_1;
                                    else
                                        pg.multiple *= -1;
                                    invalidOptions = null;
                                    break;
                                }
                            case 1:
                                var nextParameter = nextRandomIntegerExcluding_1.default(inputParamCount, pg.id);
                                var newPG = new ParameterGene_1.default(nextParameter);
                                if (isRoot)
                                    newGenome.root = newPG;
                                else
                                    parent_1.replace(gene, newPG);
                                invalidOptions = null;
                                break;
                            case 2:
                                {
                                    var newFn_1 = Operator_1.default.getRandomOperation(Operator.DIVIDE);
                                    if (isRoot)
                                        newGenome.root = newFn_1;
                                    else
                                        parent_1.replace(gene, newFn_1);
                                    newFn_1.add(gene);
                                    newFn_1.add(gene.clone());
                                    invalidOptions = null;
                                    break;
                                }
                            case 3:
                                {
                                    if (Integer_1.default.random.nextInRange(0, 3) != 0) {
                                        invalidOptions.push(4);
                                        break;
                                    }
                                    var newFn_2 = new Operator_1.default(Operator_1.default.getRandomFunctionOperator());
                                    if (isRoot)
                                        newGenome.root = newFn_2;
                                    else
                                        parent_1.replace(gene, newFn_2);
                                    newFn_2.add(gene);
                                    invalidOptions = null;
                                    break;
                                }
                            default:
                                if (parentOp.count < 3) {
                                    if (parentOp.asEnumerable().all(function (o) { return o instanceof ParameterGene_1.default || o instanceof ConstantGene_1.default; }))
                                        doNotRemove = true;
                                    else {
                                        replacement = parentOp.asEnumerable().where(function (o) { return o instanceof Operator_1.default; }).single();
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
                                currentOperatorIndex = Operator.Available.Operators.indexOf(og_1.operator);
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
                                newOperatorIndex = nextRandomIntegerExcluding_1.default(Operator.Available.Operators.length, currentOperatorIndex);
                                if (og_1.count > 2 && Integer_1.default.random.next(og_1.count) != 0) {
                                    var startIndex_1 = Integer_1.default.random.next(og_1.count - 1);
                                    var endIndex_1 = startIndex_1 == 0
                                        ? Integer_1.default.random.nextInRange(1, og_1.count - 1)
                                        : Integer_1.default.random.nextInRange(startIndex_1 + 1, og_1.count);
                                    og_1.modifyChildren(function (v) {
                                        var contents = Linq_1.default.from(v)
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
                                og_1.add(new ParameterGene_1.default(Integer_1.default.random.next(inputParamCount)));
                                invalidOptions = null;
                                break;
                            case 3:
                                first = new ParameterGene_1.default(Integer_1.default.random.next(inputParamCount));
                                newOp = inputParamCount == 1
                                    ? Operator_1.default.getRandomOperation('/')
                                    : Operator_1.default.getRandomOperation();
                                newOp.add(first);
                                if (newOp.operator == Operator.DIVIDE)
                                    newOp.add(new ParameterGene_1.default(nextRandomIntegerExcluding_1.default(inputParamCount, first.id)));
                                else
                                    newOp.add(new ParameterGene_1.default(Integer_1.default.random.next(inputParamCount)));
                                og_1.add(newOp);
                                invalidOptions = null;
                                break;
                            case 4:
                                {
                                    if (Integer_1.default.random.next(4) != 1) {
                                        break;
                                    }
                                    if (Operator.Available.Functions.indexOf(og_1.operator) != -1 && Integer_1.default.random.next(4) != 1) {
                                        break;
                                    }
                                    newFn = new Operator_1.default(Operator_1.default.getRandomFunctionOperator());
                                    if (newFn.operator == og_1.operator) {
                                        if (Integer_1.default.random.next(7) != 1) {
                                            invalidOptions.push(5);
                                            break;
                                        }
                                    }
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
                                            newGenome.root = og_1.asEnumerable().first();
                                        else {
                                            doNotRemove = true;
                                            break;
                                        }
                                    }
                                    else {
                                        parentOp.modifyChildren(function (v) {
                                            var index = v.indexOf(gene);
                                            if (index != -1) {
                                                for (var _i = 0, _a = og_1.toArray().reverse(); _i < _a.length; _i++) {
                                                    var o = _a[_i];
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
                                if (isRoot && og_1.count > 2) {
                                    og_1.removeAt(Integer_1.default.random.next(og_1.count));
                                }
                                else if (og_1.count == 2
                                    && og_1.asEnumerable().any(function (o) { return o instanceof Operator_1.default; })
                                    && og_1.asEnumerable().any(function (o) { return o instanceof ParameterGene_1.default; })) {
                                    var childOpGene = og_1.asEnumerable().ofType(Operator_1.default).single();
                                    og_1.remove(childOpGene);
                                    if (isRoot)
                                        newGenome.root = childOpGene;
                                    else
                                        parentOp.replace(og_1, childOpGene);
                                }
                                else if (shouldNotRemove() || og_1.count < 3) {
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
            var cg, abs, pg, replacement, currentOperatorIndex, newOperatorIndex, first, newOp, newFn;
            for (var i = 0; i < mutations; i++) {
                _loop_1();
            }
            return newGenome;
        };
        return AlgebraGenomeFactory;
    }(GenomeFactoryBase_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraGenomeFactory;
});
//# sourceMappingURL=GenomeFactory.js.map