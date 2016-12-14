"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
var Procedure = require("typescript-dotnet-umd/System/Collections/Array/Procedure");
var Types_1 = require("typescript-dotnet-umd/System/Types");
var Gene_1 = require("../Gene");
var ConstantGene_1 = require("./ConstantGene");
var ParameterGene_1 = require("./ParameterGene");
var Operator = require("../Operators");
var Utility_1 = require("typescript-dotnet-umd/System/Text/Utility");
var Random_1 = require("typescript-dotnet-umd/System/Random");
function arrange(a, b) {
    if (a instanceof ConstantGene_1.default && !(b instanceof ConstantGene_1.default))
        return 1;
    if (b instanceof ConstantGene_1.default && !(a instanceof ConstantGene_1.default))
        return -1;
    if (a.multiple < b.multiple)
        return 1;
    if (a.multiple > b.multiple)
        return -1;
    var ats = a.toString();
    var bts = b.toString();
    if (ats == bts)
        return 0;
    var ts = [ats, bts];
    ts.sort();
    return ts[0] == ats ? -1 : +1;
}
function parenGroup(contents) {
    return "(" + contents + ")";
}
function getRandomFrom(source, excluding) {
    return Random_1.Random.select.one(excluding === void (0) ? source : source.filter(function (v) { return v !== excluding; }), true);
}
function getRandomFromExcluding(source, excluding) {
    var options;
    if (excluding) {
        var ex_1 = Linq_1.default(excluding).memoize();
        options = source.filter(function (v) { return !ex_1.contains(v); });
    }
    else {
        options = source;
    }
    return getRandomFrom(options);
}
function getRandomOperator(source, excluded) {
    if (!excluded)
        return getRandomFrom(source);
    if (Types_1.default.isString(excluded))
        return getRandomFrom(source, excluded);
    else
        return getRandomFromExcluding(source, excluded);
}
var OperatorGene = (function (_super) {
    __extends(OperatorGene, _super);
    function OperatorGene(_operator, multiple) {
        if (multiple === void 0) { multiple = 1; }
        var _this = _super.call(this, multiple) || this;
        _this._operator = _operator;
        return _this;
    }
    Object.defineProperty(OperatorGene.prototype, "operator", {
        get: function () {
            return this._operator;
        },
        set: function (value) {
            if (this._operator != value) {
                this._operator = value;
                this._onModified();
            }
        },
        enumerable: true,
        configurable: true
    });
    OperatorGene.prototype.isReducible = function () {
        return true;
    };
    OperatorGene.prototype.modifyChildren = function (closure) {
        if (closure(this))
            this._onModified();
    };
    Object.defineProperty(OperatorGene.prototype, "arranged", {
        get: function () {
            var s = this._source, children = s.slice();
            if (s.length > 1 && this._operator != Operator.DIVIDE) {
                children.sort(arrange);
            }
            return children;
        },
        enumerable: true,
        configurable: true
    });
    OperatorGene.prototype.reduce = function (reduceGroupings) {
        if (reduceGroupings === void 0) { reduceGroupings = false; }
        var somethingDone = false;
        while (this._reduceLoop(reduceGroupings)) {
            somethingDone = true;
        }
        if (somethingDone)
            this._onModified();
        return somethingDone;
    };
    OperatorGene.prototype._reduceLoop = function (reduceGroupings) {
        if (reduceGroupings === void 0) { reduceGroupings = false; }
        var _ = this, values = _.linq;
        var somethingDone = false;
        Linq_1.default
            .from(_._source.slice())
            .groupBy(function (g) { return g.toStringContents(); })
            .where(function (g) { return g.count() > 1; })
            .forEach(function (p) {
            var pa = p.memoize();
            switch (_._operator) {
                case Operator.ADD:
                    {
                        var sum = pa.sum(function (s) { return s.multiple; });
                        var hero = pa.first();
                        if (sum == 0)
                            _._replaceInternal(hero, new ConstantGene_1.default(0));
                        else
                            hero.multiple = sum;
                        pa.skip(1).forEach(function (g) { return _._removeInternal(g); });
                        somethingDone = true;
                        break;
                    }
                case Operator.MULTIPLY:
                    {
                        var g = p
                            .ofType(OperatorGene)
                            .where(function (g) { return g.operator == Operator.SQUARE_ROOT; })
                            .toArray();
                        while (g.length > 1) {
                            _.remove(g.pop());
                            var e = g.pop();
                            _.replace(e, e.linq.single());
                        }
                        break;
                    }
                case Operator.DIVIDE:
                    {
                        var first = pa.first();
                        var next = pa.elementAt(1);
                        if (!(first instanceof ConstantGene_1.default)) {
                            _._replaceInternal(first, new ConstantGene_1.default(first.multiple));
                            _._replaceInternal(next, new ConstantGene_1.default(next.multiple));
                            somethingDone = true;
                        }
                        break;
                    }
            }
        });
        switch (_._operator) {
            case Operator.MULTIPLY:
                {
                    if (values.any(function (v) { return !v.multiple; })) {
                        _._multiple = 0;
                        somethingDone = true;
                        break;
                    }
                    var constGenes = values
                        .ofType(ConstantGene_1.default)
                        .memoize();
                    if (_.count > 1 && constGenes.any()) {
                        constGenes.forEach(function (g) {
                            _._removeInternal(g);
                            _._multiple *= g.multiple;
                            somethingDone = true;
                        });
                        if (!_.count) {
                            _._addInternal(new ConstantGene_1.default(_._multiple));
                            _._multiple = 1;
                            somethingDone = true;
                        }
                    }
                    values.where(function (v) { return v.multiple != 1; })
                        .forEach(function (p) {
                        somethingDone = true;
                        _.multiple *= p.multiple;
                        p.multiple = 1;
                    });
                    var mParams_1 = values
                        .ofType(ParameterGene_1.default)
                        .memoize();
                    if (mParams_1.any()) {
                        var divOperator = values
                            .ofType(OperatorGene)
                            .where(function (g) {
                            return g.operator == Operator.DIVIDE &&
                                g.linq
                                    .skip(1)
                                    .ofType(ParameterGene_1.default)
                                    .any(function (g2) {
                                    return mParams_1.any(function (g3) { return g2.id == g3.id; });
                                });
                        })
                            .firstOrDefault();
                        if (divOperator != null) {
                            var oneToKill_1 = divOperator.linq
                                .skip(1)
                                .ofType(ParameterGene_1.default)
                                .where(function (p) { return mParams_1.any(function (pn) { return pn.id == p.id; }); })
                                .first();
                            var mPToKill = mParams_1
                                .where(function (p) { return p.id == oneToKill_1.id; }).first();
                            divOperator.replace(oneToKill_1, new ConstantGene_1.default(oneToKill_1.multiple));
                            _._replaceInternal(mPToKill, new ConstantGene_1.default(mPToKill.multiple));
                            somethingDone = true;
                        }
                    }
                }
                break;
            case Operator.DIVIDE:
                {
                    var f = values.firstOrDefault();
                    if (f != null && f.multiple != 1) {
                        _._multiple *= f.multiple;
                        f.multiple = 1;
                        somethingDone = true;
                        break;
                    }
                    if (f && f.multiple == 0) {
                        _._multiple = 0;
                        somethingDone = true;
                        break;
                    }
                    if (values.skip(1).any(function (v) { return v.multiple == 0; })) {
                        _._multiple = NaN;
                        somethingDone = true;
                    }
                    {
                        var multiple_1 = _._multiple;
                        if (multiple_1 == 0 || isNaN(multiple_1))
                            break;
                    }
                    values.where(function (v) { return v.multiple < 0; }).forEach(function (p) {
                        somethingDone = true;
                        _._multiple *= -1;
                        p.multiple *= -1;
                    });
                    {
                        var newConst_1 = values
                            .ofType(ConstantGene_1.default)
                            .lastOrDefault();
                        var havMultiples = values
                            .skip(1)
                            .where(function (v) { return v.multiple != 1 && v != newConst_1; }).toArray();
                        if (havMultiples.length) {
                            if (!newConst_1) {
                                newConst_1 = new ConstantGene_1.default(1);
                                _._addInternal(newConst_1);
                            }
                            for (var _i = 0, havMultiples_1 = havMultiples; _i < havMultiples_1.length; _i++) {
                                var g = havMultiples_1[_i];
                                newConst_1.multiple *= g.multiple;
                                g.multiple = 1;
                                if (g instanceof ConstantGene_1.default)
                                    _._removeInternal(g);
                            }
                            somethingDone = true;
                        }
                    }
                    if (f instanceof OperatorGene) {
                        var mSource = void 0;
                        switch (f.operator) {
                            case Operator.MULTIPLY:
                                mSource = f.linq;
                                break;
                            case Operator.DIVIDE:
                                mSource = f.linq.take(1);
                                break;
                            default:
                                mSource = Linq_1.default.empty();
                                break;
                        }
                        var mParams_2 = mSource
                            .ofType(ParameterGene_1.default)
                            .memoize();
                        if (mParams_2.any()) {
                            var oneToKill_2 = values
                                .ofType(ParameterGene_1.default)
                                .where(function (p) { return mParams_2.any(function (pn) { return pn.id == p.id; }); })
                                .firstOrDefault();
                            if (oneToKill_2 != null) {
                                var mPToKill = mParams_2.where(function (p) { return p.id == oneToKill_2.id; }).first();
                                _._replaceInternal(oneToKill_2, new ConstantGene_1.default(oneToKill_2.multiple));
                                f.replace(mPToKill, new ConstantGene_1.default(mPToKill.multiple));
                                somethingDone = true;
                            }
                        }
                    }
                    if (f instanceof ParameterGene_1.default) {
                        var fP_1 = f;
                        var multiplyOperator = values
                            .ofType(OperatorGene)
                            .where(function (g) { return g.operator == Operator.MULTIPLY
                            && g.linq
                                .ofType(ParameterGene_1.default)
                                .any(function (g2) { return g2.id == fP_1.id; }); })
                            .firstOrDefault();
                        if (multiplyOperator != null) {
                            var oneToKill = multiplyOperator.linq
                                .ofType(ParameterGene_1.default)
                                .where(function (p) { return p.id == fP_1.id; })
                                .first();
                            _._replaceInternal(f, new ConstantGene_1.default(f.multiple));
                            multiplyOperator.replace(oneToKill, new ConstantGene_1.default(oneToKill.multiple));
                            somethingDone = true;
                        }
                        else {
                            var divOperator = values
                                .ofType(OperatorGene)
                                .where(function (g) {
                                return g.operator == Operator.DIVIDE &&
                                    g.linq.take(1)
                                        .ofType(ParameterGene_1.default)
                                        .any(function (g2) { return g2.id == fP_1.id; });
                            })
                                .firstOrDefault();
                            if (divOperator != null) {
                                var oneToKill = divOperator.linq.first();
                                _._replaceInternal(f, new ConstantGene_1.default(f.multiple));
                                divOperator.replace(oneToKill, new ConstantGene_1.default(oneToKill.multiple));
                                somethingDone = true;
                            }
                        }
                    }
                }
                {
                    var divisors = values.skip(1), g = void 0;
                    while (g = divisors.ofType(ConstantGene_1.default)
                        .where(function (p) { return _._multiple % p.multiple == 0; })
                        .firstOrDefault()) {
                        _._multiple /= g.multiple;
                        _._removeInternal(g);
                        somethingDone = true;
                    }
                    while (g
                        = divisors
                            .where(function (p) { return p.multiple != 1 && _._multiple % p.multiple == 0; })
                            .firstOrDefault()) {
                        _._multiple /= g.multiple;
                        g.multiple = 1;
                        somethingDone = true;
                    }
                }
                for (var _a = 0, _b = values
                    .skip(1).ofType(OperatorGene)
                    .where(function (p) { return p.operator == Operator.DIVIDE && p.count > 1; })
                    .take(1).toArray(); _a < _b.length; _a++) {
                    var g = _b[_a];
                    var f = values.first();
                    var fo = f instanceof OperatorGene ? f : null;
                    if (!fo || fo.operator != Operator.MULTIPLY) {
                        fo = new OperatorGene(Operator.MULTIPLY);
                        _._replaceInternal(f, fo);
                        fo.add(f);
                        somethingDone = true;
                    }
                    for (var _c = 0, _d = g.linq.skip(1).toArray(); _c < _d.length; _c++) {
                        var n = _d[_c];
                        g.remove(n);
                        fo.add(n);
                        somethingDone = true;
                    }
                }
                break;
        }
        var multiple = _._multiple;
        if (multiple == 0 || !isFinite(multiple) || isNaN(multiple)) {
            if (values.any()) {
                _._clearInternal();
                somethingDone = true;
            }
        }
        for (var _e = 0, _f = values
            .ofType(OperatorGene)
            .toArray(); _e < _f.length; _e++) {
            var o = _f[_e];
            if (!values.contains(o))
                continue;
            if (o.reduce(reduceGroupings))
                somethingDone = true;
            if (o.count == 0) {
                somethingDone = true;
                _._replaceInternal(o, new ConstantGene_1.default(o.multiple));
                continue;
            }
            if (reduceGroupings) {
                if (_._operator == Operator.ADD) {
                    if (o.multiple == 0) {
                        _._removeInternal(o);
                        somethingDone = true;
                        continue;
                    }
                    if (o.operator == Operator.ADD) {
                        var index = _._source.indexOf(o);
                        for (var _g = 0, _h = o.toArray(); _g < _h.length; _g++) {
                            var og = _h[_g];
                            o.remove(og);
                            og.multiple *= o.multiple;
                            _.insert(index, og);
                        }
                        _._removeInternal(o);
                        somethingDone = true;
                        continue;
                    }
                }
                if (_._operator == Operator.MULTIPLY) {
                    if (o.operator == Operator.MULTIPLY) {
                        _._multiple *= o._multiple;
                        var index = _._source.indexOf(o);
                        for (var _j = 0, _k = o.toArray(); _j < _k.length; _j++) {
                            var og = _k[_j];
                            o.remove(og);
                            _.insert(index, og);
                        }
                        _._removeInternal(o);
                        somethingDone = true;
                        continue;
                    }
                }
            }
            if (o.count == 1) {
                var opg = o.linq.firstOrDefault();
                if (opg instanceof ParameterGene_1.default || opg instanceof ConstantGene_1.default) {
                    if (Operator.Available.Functions.indexOf(o.operator) == -1) {
                        o.remove(opg);
                        opg.multiple *= o.multiple;
                        this._replaceInternal(o, opg);
                        somethingDone = true;
                        continue;
                    }
                    else if (o.operator == Operator.SQUARE_ROOT && opg instanceof ConstantGene_1.default && opg.multiple >= 0) {
                        var sq = Math.sqrt(opg.multiple);
                        for (var i = 0; i < sq; i++) {
                            if (i == sq) {
                                o.remove(opg);
                                this._replaceInternal(o, opg);
                                somethingDone = true;
                            }
                        }
                    }
                }
                if (opg instanceof OperatorGene) {
                    var og = opg;
                    if (o.operator == Operator.SQUARE_ROOT) {
                        var childCount = og.count;
                        if (og.operator == Operator.MULTIPLY && childCount == 2
                            && og.linq.select(function (p) { return p.asReduced().toString(); })
                                .distinct()
                                .count() == 1) {
                            var firstChild = og.linq.first();
                            og.remove(firstChild);
                            o.operator = Operator.MULTIPLY;
                            o.clear();
                            o.add(firstChild);
                            somethingDone = true;
                        }
                    }
                    else if (Operator.Available.Operators.indexOf(o.operator) != -1) {
                        var children = og.toArray();
                        o.operator = og.operator;
                        o.multiple *= og.multiple;
                        og.clear();
                        o.clear();
                        o.importEntries(children);
                        somethingDone = true;
                    }
                }
            }
        }
        if (_.count == 1) {
            var p = values.first();
            if (_._operator == Operator.SQUARE_ROOT) {
                if (p instanceof ConstantGene_1.default) {
                    if (p.multiple == 0) {
                        somethingDone = true;
                        _.multiple *= p.multiple;
                        p.multiple = 1;
                    }
                }
            }
            else {
                if (!(p instanceof ConstantGene_1.default) && p.multiple !== 1) {
                    somethingDone = true;
                    _.multiple *= p.multiple;
                    p.multiple = 1;
                }
            }
        }
        if (_._operator == Operator.ADD && _._source.length > 1) {
            var constants = Linq_1.default
                .from(_._source.slice())
                .ofType(ConstantGene_1.default);
            var len = constants.count();
            if (len) {
                var sum = constants.sum(function (s) { return s.multiple; });
                var hero = constants.first();
                if (len > 1) {
                    hero.multiple = sum;
                    constants.skip(1).forEach(function (c) { return _._removeInternal(c); });
                    len = 1;
                    somethingDone = true;
                }
                if (sum == 0 && _._source.length > len) {
                    _._removeInternal(hero);
                    somethingDone = true;
                }
            }
        }
        if (_._operator != Operator.DIVIDE) {
            _._source.sort(arrange);
        }
        if (somethingDone)
            _._onModified();
        return somethingDone;
    };
    OperatorGene.prototype.asReduced = function () {
        var r = this._reduced;
        if (!r) {
            var gene = this.clone();
            gene.reduce(true);
            this._reduced = r = gene.toString() === this.toString() ? this : gene;
        }
        return r;
    };
    OperatorGene.prototype.resetToString = function () {
        this._reduced = void 0;
        _super.prototype.resetToString.call(this);
    };
    OperatorGene.prototype.toStringContents = function () {
        var _ = this;
        if (Operator.Available.Functions.indexOf(_._operator) != -1) {
            if (_._source.length == 1)
                return _.operator
                    + _._source[0].toString();
            else
                return _.operator
                    + parenGroup(_.arranged.map(function (s) { return s.toString(); }).join(","));
        }
        if (_._operator == Operator.ADD) {
            return parenGroup(Utility_1.trim(_.arranged.map(function (s) {
                var r = s.toString();
                if (!Utility_1.startsWith(r, '-'))
                    r = "+" + r;
                return r;
            }).join(""), "+"));
        }
        else
            return parenGroup(_.arranged.map(function (s) { return s.toString(); }).join(_.operator));
    };
    OperatorGene.prototype.toEntityWithoutMultiple = function () {
        var _ = this;
        var fni = Operator.Available.Functions.indexOf(_._operator);
        if (fni != -1) {
            if (!_._source.length)
                return "NaN";
            return Operator.Available.FunctionActual[fni]
                + parenGroup(_.arranged.map(function (s) { return s.toEntity(); }).join(","));
        }
        if (_._operator == Operator.ADD) {
            return parenGroup(Utility_1.trim(_.arranged.map(function (s) {
                var r = s.toEntity();
                if (!Utility_1.startsWith(r, '-'))
                    r = "+" + r;
                return r;
            }).join(""), "+"));
        }
        else
            return parenGroup(_.arranged.map(function (s) { return s.toEntity(); }).join(_.operator));
    };
    OperatorGene.prototype.clone = function () {
        var clone = new OperatorGene(this._operator, this._multiple);
        this.forEach(function (g) {
            clone._addInternal(g.clone());
        });
        return clone;
    };
    OperatorGene.getRandomOperator = function (excluded) {
        return getRandomOperator(Operator.Available.Operators, excluded);
    };
    OperatorGene.getRandomFunctionOperator = function (excluded) {
        return getRandomOperator(Operator.Available.Functions, excluded);
    };
    OperatorGene.getRandomOperation = function (excluded) {
        return new OperatorGene(OperatorGene.getRandomOperator(excluded));
    };
    OperatorGene.prototype.calculateWithoutMultiple = function (values) {
        var results = this._source.map(function (s) { return s.calculate(values); });
        switch (this._operator) {
            case Operator.ADD:
                return Procedure.sum(results);
            case Operator.MULTIPLY:
                return Procedure.product(results);
            case Operator.DIVIDE:
                return Procedure.quotient(results);
            case Operator.SQUARE_ROOT:
                if (results.length == 1)
                    return Math.sqrt(results[0]);
                break;
        }
        return NaN;
    };
    return OperatorGene;
}(Gene_1.default));
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = OperatorGene;
//# sourceMappingURL=Operator.js.map