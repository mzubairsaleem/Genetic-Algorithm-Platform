var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments)).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t;
    return { next: verb(0), "throw": verb(1), "return": verb(2) };
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
(function (dependencies, factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(dependencies, factory);
    }
})(["require", "exports", "typescript-dotnet-umd/System/Collections/Set", "typescript-dotnet-umd/System/Collections/Dictionaries/StringKeyDictionary", "../../arithmetic/Correlation", "../../source/Fitness", "typescript-dotnet-umd/System.Linq/Linq", "typescript-dotnet-umd/System/Collections/Array/Procedure", "typescript-dotnet-umd/System/Promises/Promise", "typescript-dotnet-umd/System/Threading/Tasks/Parallel", "typescript-dotnet-umd/System/Text/Utility"], function (require, exports) {
    "use strict";
    var Set_1 = require("typescript-dotnet-umd/System/Collections/Set");
    var StringKeyDictionary_1 = require("typescript-dotnet-umd/System/Collections/Dictionaries/StringKeyDictionary");
    var Correlation_1 = require("../../arithmetic/Correlation");
    var Fitness_1 = require("../../source/Fitness");
    var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
    var Procedure_1 = require("typescript-dotnet-umd/System/Collections/Array/Procedure");
    var Promise_1 = require("typescript-dotnet-umd/System/Promises/Promise");
    var Parallel_1 = require("typescript-dotnet-umd/System/Threading/Tasks/Parallel");
    var Utility_1 = require("typescript-dotnet-umd/System/Text/Utility");
    var S_INDEXES = Object.freeze(Linq_1.default.range(0, 30).select(function (n) { return "s[" + n + "]"; }).toArray());
    var AlgebraBlackBoxProblem = (function () {
        function AlgebraBlackBoxProblem(actualFormula) {
            this._fitness = {};
            this._actualFormula = actualFormula;
            this._convergent = new StringKeyDictionary_1.default();
        }
        Object.defineProperty(AlgebraBlackBoxProblem.prototype, "convergent", {
            get: function () {
                return this._convergent.values;
            },
            enumerable: true,
            configurable: true
        });
        AlgebraBlackBoxProblem.prototype.getFitnessFor = function (genome, createIfMissing) {
            if (createIfMissing === void 0) { createIfMissing = true; }
            var h = genome.hashReduced, f = this._fitness;
            var s = f[h];
            if (!s && createIfMissing)
                f[h] = s = new Fitness_1.default();
            return s;
        };
        AlgebraBlackBoxProblem.prototype.rank = function (population) {
            var _this = this;
            return Linq_1.default(population)
                .orderByDescending(function (g) { return _this.getFitnessFor(g); })
                .thenBy(function (g) { return g.hash.length; });
        };
        AlgebraBlackBoxProblem.prototype.rankAndReduce = function (population, targetMaxPopulation) {
            var _this = this;
            var lastFitness;
            return this.rank(population)
                .takeWhile(function (g, i) {
                var lf = lastFitness, f = _this.getFitnessFor(g);
                lastFitness = f;
                return i < targetMaxPopulation || lf.compareTo(f) === 0;
            });
        };
        AlgebraBlackBoxProblem.prototype.correlation = function (aSample, bSample, gA, gB) {
            return __awaiter(this, void 0, Promise_1.Promise, function () {
                var len, gA_result, gB_result, i, _i, _a, a, _b, _c, b, params;
                return __generator(this, function (_d) {
                    len = aSample.length * bSample.length;
                    gA_result = new Float64Array(len);
                    gB_result = new Float64Array(len);
                    i = 0;
                    for (_i = 0, _a = aSample; _i < _a.length; _i++) {
                        a = _a[_i];
                        for (_b = 0, _c = bSample; _b < _c.length; _b++) {
                            b = _c[_b];
                            params = [a, b];
                            gA_result[i] = gA.calculate(params);
                            gB_result[i] = gB.calculate(params);
                            i++;
                        }
                    }
                    return [2 /*return*/, Correlation_1.correlation(gA_result, gB_result)];
                });
            });
        };
        AlgebraBlackBoxProblem.prototype.sample = function (count, range) {
            if (count === void 0) { count = 5; }
            if (range === void 0) { range = 100; }
            var result = new Set_1.default();
            while (result.count < count) {
                result.add(Math.random() * range);
            }
            var a = result.toArray();
            a.sort();
            return a;
        };
        AlgebraBlackBoxProblem.prototype.test = function (p, count) {
            if (count === void 0) { count = 1; }
            return __awaiter(this, void 0, Promise_1.Promise, function () {
                var f, result, genomes, i, aSample, bSample, correct, _i, aSample_1, a, _a, bSample_1, b, results, i_1, len, g, calc, divergence, len_1, i_2, c, d, f_1;
                return __generator(this, function (_b) {
                    switch (_b.label) {
                        case 0:
                            f = this._actualFormula;
                            result = [];
                            genomes = p.toArray();
                            i = 0;
                            _b.label = 1;
                        case 1:
                            if (!(i < count))
                                return [3 /*break*/, 4];
                            aSample = this.sample();
                            bSample = this.sample();
                            correct = [];
                            for (_i = 0, aSample_1 = aSample; _i < aSample_1.length; _i++) {
                                a = aSample_1[_i];
                                for (_a = 0, bSample_1 = bSample; _a < bSample_1.length; _a++) {
                                    b = bSample_1[_a];
                                    correct.push(f(a, b));
                                }
                            }
                            return [4 /*yield*/, Parallel_1.Parallel.options({
                                    maxConcurrency: 4
                                })
                                    .startNew({
                                    fns: genomes.map(function (g) { return Utility_1.supplant(g.toEntity(), S_INDEXES).replace("()", "NaN"); }),
                                    source: [aSample, bSample]
                                }, function (data) {
                                    var fns = data.fns, source = data.source, result = [];
                                    var aSample = source[0], bSample = source[1];
                                    var samples = [];
                                    for (var _i = 0, aSample_2 = aSample; _i < aSample_2.length; _i++) {
                                        var a = aSample_2[_i];
                                        for (var _a = 0, bSample_2 = bSample; _a < bSample_2.length; _a++) {
                                            var b = bSample_2[_a];
                                            samples.push([a, b]);
                                        }
                                    }
                                    var _loop_1 = function (f_2) {
                                        var calc = void 0;
                                        try {
                                            calc = samples.map(function (s) { return eval(f_2); });
                                        }
                                        catch (ex) {
                                            calc = samples.map(function (s) { return NaN; });
                                            console.error("Bad Function:", f_2);
                                            console.error(ex);
                                        }
                                        result.push(calc);
                                    };
                                    for (var _b = 0, fns_1 = fns; _b < fns_1.length; _b++) {
                                        var f_2 = fns_1[_b];
                                        _loop_1(f_2);
                                    }
                                    return result;
                                })];
                        case 2:
                            results = _b.sent();
                            for (i_1 = 0, len = genomes.length; i_1 < len; i_1++) {
                                g = genomes[i_1];
                                calc = results[i_1];
                                divergence = [];
                                len_1 = correct.length;
                                divergence.length = correct.length;
                                for (i_2 = 0; i_2 < len_1; i_2++) {
                                    divergence[i_2] = -Math.abs(calc[i_2] - correct[i_2]);
                                }
                                c = Correlation_1.correlation(correct, calc);
                                d = Procedure_1.average(divergence) + 1;
                                f_1 = this.getFitnessFor(g);
                                f_1.addScores((isNaN(c) || !isFinite(c)) ? -2 : c, (isNaN(d) || !isFinite(d)) ? -Infinity : d);
                                this._convergent.setValue(g.hashReduced, f_1.hasConverged()
                                    ? g
                                    : (void 0));
                            }
                            _b.label = 3;
                        case 3:
                            i++;
                            return [3 /*break*/, 1];
                        case 4: return [4 /*yield*/, result];
                        case 5:
                            _b.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        return AlgebraBlackBoxProblem;
    }());
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraBlackBoxProblem;
});
//# sourceMappingURL=Problem.js.map