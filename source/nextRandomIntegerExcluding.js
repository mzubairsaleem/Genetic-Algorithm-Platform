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
})(["require", "exports", "typescript-dotnet-umd/System/Types", "typescript-dotnet-umd/System/Integer", "typescript-dotnet-umd/System/Collections/Set", "typescript-dotnet-umd/System/Exceptions/ArgumentOutOfRangeException", "typescript-dotnet-umd/System/Random"], function (require, exports) {
    "use strict";
    var Types_1 = require("typescript-dotnet-umd/System/Types");
    var Integer_1 = require("typescript-dotnet-umd/System/Integer");
    var Set_1 = require("typescript-dotnet-umd/System/Collections/Set");
    var ArgumentOutOfRangeException_1 = require("typescript-dotnet-umd/System/Exceptions/ArgumentOutOfRangeException");
    var Random_1 = require("typescript-dotnet-umd/System/Random");
    function nextRandomIntegerExcluding(range, excluding) {
        Integer_1.Integer.assert(range);
        if (range < 0)
            throw new ArgumentOutOfRangeException_1.ArgumentOutOfRangeException("range", range, "Must be a number greater than zero.");
        var r = [], excludeSet = new Set_1.Set(Types_1.Type.isNumber(excluding, true) ? [excluding] : excluding);
        for (var i = 0; i < range; ++i) {
            if (!excludeSet.contains(i))
                r.push(i);
        }
        return Random_1.Random.select.one(r, true);
    }
    exports.nextRandomIntegerExcluding = nextRandomIntegerExcluding;
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = nextRandomIntegerExcluding;
});
//# sourceMappingURL=nextRandomIntegerExcluding.js.map