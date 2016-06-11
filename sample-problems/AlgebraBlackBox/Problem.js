/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
"use strict";
var Set_1 = require("typescript-dotnet-umd/System/Collections/Set");
var StringKeyDictionary_1 = require("typescript-dotnet-umd/System/Collections/Dictionaries/StringKeyDictionary");
var ArrayUtility = require("typescript-dotnet-umd/System/Collections/Array/Utility");
var Correlation_1 = require("./arithmetic/Correlation");
var Fitness_1 = require("../../source/Fitness");
var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
var Procedure_1 = require("typescript-dotnet-umd/System/Collections/Array/Procedure");
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
        var h = genome.hashReduced, f = this._fitness, s = f[h];
        if (!s && createIfMissing)
            f[h] = s = new Fitness_1.default();
        return s;
    };
    AlgebraBlackBoxProblem.prototype.rank = function (population) {
        var _this = this;
        return Linq_1.default
            .from(population)
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
        var len = aSample.length * bSample.length;
        var gA_result = ArrayUtility.initialize(len);
        var gB_result = ArrayUtility.initialize(len);
        for (var _i = 0, aSample_1 = aSample; _i < aSample_1.length; _i++) {
            var a = aSample_1[_i];
            for (var _a = 0, bSample_1 = bSample; _a < bSample_1.length; _a++) {
                var b = bSample_1[_a];
                gA_result.push(gA.calculate([a, b]));
                gB_result.push(gB.calculate([a, b]));
            }
        }
        return Correlation_1.correlation(gA_result, gB_result);
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
        var _this = this;
        if (count === void 0) { count = 1; }
        var f = this._actualFormula;
        var _loop_1 = function(i) {
            var aSample = this_1.sample();
            var bSample = this_1.sample();
            var correct = [];
            for (var _i = 0, aSample_2 = aSample; _i < aSample_2.length; _i++) {
                var a = aSample_2[_i];
                for (var _a = 0, bSample_2 = bSample; _a < bSample_2.length; _a++) {
                    var b = bSample_2[_a];
                    correct.push(f(a, b));
                }
            }
            p.forEach(function (g) {
                var result = [];
                for (var _i = 0, aSample_3 = aSample; _i < aSample_3.length; _i++) {
                    var a = aSample_3[_i];
                    for (var _a = 0, bSample_3 = bSample; _a < bSample_3.length; _a++) {
                        var b = bSample_3[_a];
                        result.push(g.calculate([a, b]));
                    }
                }
                var divergence = [];
                var len = correct.length;
                divergence.length = correct.length;
                for (var i_1 = 0; i_1 < len; i_1++) {
                    divergence[i_1] = -Math.abs(result[i_1] - correct[i_1]);
                }
                var c = Correlation_1.correlation(correct, result);
                var d = Procedure_1.average(divergence) + 1;
                var f = _this.getFitnessFor(g);
                f.add([
                    (isNaN(c) || !isFinite(c)) ? -2 : c,
                    (isNaN(d) || !isFinite(d)) ? -Infinity : d
                ]);
                _this._convergent.setValue(g.hashReduced, f.hasConverged() ? g : (void 0));
            });
        };
        var this_1 = this;
        for (var i = 0; i < count; i++) {
            _loop_1(i);
        }
    };
    return AlgebraBlackBoxProblem;
}());
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = AlgebraBlackBoxProblem;
//# sourceMappingURL=Problem.js.map