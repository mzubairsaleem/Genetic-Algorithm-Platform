/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports", "../node_modules/typescript-dotnet/source/System/Collections/Set", "../node_modules/typescript-dotnet/source/System/Collections/Array/Utility", "../source/arithmetic/Correlation", "./AlgebraGenomeFactory", "../node_modules/typescript-dotnet/source/System.Linq/Linq"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Set_1 = require("../node_modules/typescript-dotnet/source/System/Collections/Set");
    var ArrayUtility = require("../node_modules/typescript-dotnet/source/System/Collections/Array/Utility");
    var Correlation_1 = require("../source/arithmetic/Correlation");
    var AlgebraGenomeFactory_1 = require("./AlgebraGenomeFactory");
    var Linq_1 = require("../node_modules/typescript-dotnet/source/System.Linq/Linq");
    function actualFormula(a, b) {
        return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
    }
    var AlgebraBlackBoxProblem = (function () {
        function AlgebraBlackBoxProblem() {
        }
        AlgebraBlackBoxProblem.prototype.rank = function (population) {
            return Linq_1.default
                .from(population)
                .orderByDescending(function (o) { return o.fitness.score; });
        };
        AlgebraBlackBoxProblem.prototype.rankAndReduce = function (population, targetMaxPopulation) {
            var lastValue;
            return this.rank(population)
                .takeWhile(function (o, i) {
                var lv = lastValue, s = o.fitness.score;
                lastValue = s;
                return i < targetMaxPopulation || lv === s;
            });
        };
        AlgebraBlackBoxProblem.prototype.getGenomeFactory = function () {
            return new AlgebraGenomeFactory_1.default(2);
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
                p.forEach(function (o) {
                    var result = [];
                    for (var _i = 0, aSample_3 = aSample; _i < aSample_3.length; _i++) {
                        var a = aSample_3[_i];
                        for (var _a = 0, bSample_3 = bSample; _a < bSample_3.length; _a++) {
                            var b = bSample_3[_a];
                            result.push(o.genome.calculate([a, b]));
                        }
                    }
                    o.fitness.add(Correlation_1.correlation(correct, result));
                });
            }
        };
        return AlgebraBlackBoxProblem;
    }());
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraBlackBoxProblem;
});
//# sourceMappingURL=AlgebraBlackBoxProblem.js.map