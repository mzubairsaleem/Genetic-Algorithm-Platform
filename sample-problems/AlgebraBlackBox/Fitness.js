"use strict";
var Procedure = require("typescript-dotnet/source/System/Collections/Array/Procedure");
var ReadOnlyArrayWrapper_1 = require("typescript-dotnet/source/System/Collections/Array/ReadOnlyArrayWrapper");
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
//# sourceMappingURL=Fitness.js.map