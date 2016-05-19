/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports", "typescript-dotnet/source/System/Collections/Array/Procedure", "typescript-dotnet/source/System/Collections/Enumeration/Enumerator", "typescript-dotnet/source/System/Exception"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Procedure_1 = require("typescript-dotnet/source/System/Collections/Array/Procedure");
    var Enumerator_1 = require("typescript-dotnet/source/System/Collections/Enumeration/Enumerator");
    var Exception_1 = require("typescript-dotnet/source/System/Exception");
    function abs(source) {
        return source.map(function (v) { return isNaN(v) ? v : Math.abs(v); });
    }
    exports.abs = abs;
    function deltas(source) {
        var previous = NaN;
        return source.map(function (v) {
            if (!isNaN(v)) {
                var p = previous;
                previous = v;
                if (!isNaN(p)) {
                    return v - p;
                }
            }
        });
    }
    exports.deltas = deltas;
    function variance(source) {
        return Procedure_1.average(source.map(function (s) { return s * s; })) - Math.pow(Procedure_1.average(source), 2);
    }
    exports.variance = variance;
    function products(source, target) {
        var sourceEnumerator = Enumerator_1.from(source);
        var targetEnumerator = Enumerator_1.from(target);
        var result = [];
        while (true) {
            var sv = sourceEnumerator.moveNext();
            var tv = targetEnumerator.moveNext();
            if (sv != tv)
                throw new Exception_1.default("Products: source and target enumerations have different counts.");
            if (!sv || !tv)
                break;
            result.push(sourceEnumerator.current * targetEnumerator.current);
        }
        return result;
    }
    exports.products = products;
    function covariance(source, target) {
        return Procedure_1.average(products(source, target)) - Procedure_1.average(source) * Procedure_1.average(target);
    }
    exports.covariance = covariance;
    function correlationUsing(covariance, sourceVariance, targetVariance) {
        return covariance / Math.sqrt(sourceVariance * targetVariance);
    }
    exports.correlationUsing = correlationUsing;
    function correlationOf(covariance, source, target) {
        return correlationUsing(covariance, variance(source), variance(target));
    }
    exports.correlationOf = correlationOf;
    function correlation(source, target) {
        return correlationOf(covariance(source, target), source, target);
    }
    exports.correlation = correlation;
});
//# sourceMappingURL=Correlation.js.map