/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
(function (dependencies, factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(dependencies, factory);
    }
})(["require", "exports", "typescript-dotnet-umd/System.Linq/Linq"], function (require, exports) {
    "use strict";
    var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
    function forward(n) {
        return n * (n + 1) / 2;
    }
    exports.forward = forward;
    function reverse(n) {
        return (Math.sqrt(8 * n + 1) - 1) / 2 | 0;
    }
    exports.reverse = reverse;
    var disperse;
    (function (disperse) {
        function increasing(source) {
            return Linq_1.Enumerable(source)
                .selectMany(function (c, i) { return Linq_1.Enumerable.repeat(c, i + 1); });
        }
        disperse.increasing = increasing;
        function decreasing(source) {
            var s = Linq_1.Enumerable(source).memoize();
            return Linq_1.Enumerable(source)
                .selectMany(function (c, i) { return s.take(i + 1); });
        }
        disperse.decreasing = decreasing;
    })(disperse = exports.disperse || (exports.disperse = {}));
});
//# sourceMappingURL=Triangular.js.map