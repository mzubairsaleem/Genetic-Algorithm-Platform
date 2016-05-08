/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports", "../../node_modules/typescript-dotnet/source/System/Collections/Set", "../../node_modules/typescript-dotnet/source/System/Collections/Array/Utility", "./arithmetic/Correlation", "./Fitness", "../../node_modules/typescript-dotnet/source/System.Linq/Linq", "../../node_modules/typescript-dotnet/source/System/Types"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Set_1 = require("../../node_modules/typescript-dotnet/source/System/Collections/Set");
    var ArrayUtility = require("../../node_modules/typescript-dotnet/source/System/Collections/Array/Utility");
    var Correlation_1 = require("./arithmetic/Correlation");
    var Fitness_1 = require("./Fitness");
    var Linq_1 = require("../../node_modules/typescript-dotnet/source/System.Linq/Linq");
    var Types_1 = require("../../node_modules/typescript-dotnet/source/System/Types");
    function actualFormula(a, b) {
        return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
    }
    var AlgebraBlackBoxProblem = (function () {
        function AlgebraBlackBoxProblem(actualFormula) {
            this._fitness = {};
            this._actualFormula = actualFormula;
        }
        AlgebraBlackBoxProblem.prototype.getScoreFor = function (genome) {
            if (!genome)
                return 0;
            if (!Types_1.default.isString(genome))
                genome = genome.hash;
            var s = this._fitness[genome];
            return s && s.score || 0;
        };
        AlgebraBlackBoxProblem.prototype.getFitnessFor = function (genome, createIfMissing) {
            if (createIfMissing === void 0) { createIfMissing = true; }
            var h = genome.hash, f = this._fitness, s = f[h];
            if (!s && createIfMissing)
                f[h] = s = new Fitness_1.default();
            return s;
        };
        AlgebraBlackBoxProblem.prototype.rank = function (population) {
            var _this = this;
            return Linq_1.default
                .from(population)
                .orderByDescending(function (g) { return _this.getScoreFor(g); });
        };
        AlgebraBlackBoxProblem.prototype.rankAndReduce = function (population, targetMaxPopulation) {
            var _this = this;
            var lastValue;
            return this.rank(population)
                .takeWhile(function (g, i) {
                var lv = lastValue, s = _this.getScoreFor(g);
                lastValue = s;
                return i < targetMaxPopulation || lv === s;
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
            for (var i = 0; i < count; i++) {
                var aSample = this.sample();
                var bSample = this.sample();
                var correct = [];
                for (var _i = 0, aSample_2 = aSample; _i < aSample_2.length; _i++) {
                    var a = aSample_2[_i];
                    for (var _a = 0, bSample_2 = bSample; _a < bSample_2.length; _a++) {
                        var b = bSample_2[_a];
                        correct.push(actualFormula(a, b));
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
                    var c = Correlation_1.correlation(correct, result);
                    _this.getFitnessFor(g)
                        .add((isNaN(c) || !isFinite(c)) ? -2 : c);
                });
            }
        };
        return AlgebraBlackBoxProblem;
    }());
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraBlackBoxProblem;
});
//# sourceMappingURL=Problem.js.map