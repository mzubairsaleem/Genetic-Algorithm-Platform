(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports", "../../node_modules/typescript-dotnet/source/System/Collections/Array/Procedure", "../../node_modules/typescript-dotnet/source/System/Collections/Array/ReadOnlyArrayWrapper"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Procedure = require("../../node_modules/typescript-dotnet/source/System/Collections/Array/Procedure");
    var ReadOnlyArrayWrapper_1 = require("../../node_modules/typescript-dotnet/source/System/Collections/Array/ReadOnlyArrayWrapper");
    var AlgebraFitness = (function () {
        function AlgebraFitness() {
            this._score = NaN;
            this._samples = [];
        }
        Object.defineProperty(AlgebraFitness.prototype, "scores", {
            get: function () {
                return this._samplesReadOnly || (this._samplesReadOnly
                    = new ReadOnlyArrayWrapper_1.default(this._samples));
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(AlgebraFitness.prototype, "count", {
            get: function () {
                return this._samples.length;
            },
            enumerable: true,
            configurable: true
        });
        AlgebraFitness.prototype.add = function (score) {
            this._samples.push(score);
            this._score = Procedure.average(this._samples);
        };
        Object.defineProperty(AlgebraFitness.prototype, "score", {
            get: function () {
                return this._score;
            },
            enumerable: true,
            configurable: true
        });
        return AlgebraFitness;
    }());
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraFitness;
});
//# sourceMappingURL=Fitness.js.map