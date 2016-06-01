"use strict";
var Procedure = require("typescript-dotnet/source/System/Collections/Array/Procedure");
var AlgebraFitness = (function () {
    function AlgebraFitness() {
        this._samples = [];
        this._scores = [];
        this._count = 0;
    }
    Object.defineProperty(AlgebraFitness.prototype, "count", {
        get: function () {
            return this._count;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(AlgebraFitness.prototype, "scores", {
        get: function () {
            return this._scores.slice();
        },
        enumerable: true,
        configurable: true
    });
    AlgebraFitness.prototype.add = function (score) {
        if (!score || !score.length)
            return;
        for (var i = 0, len = score.length; i < len; i++) {
            var s = this._samples[i];
            if (!s)
                this._samples[i] = s = [];
            s.push(score[i]);
            this._scores[i] = null;
        }
        this._count++;
    };
    AlgebraFitness.prototype.getScore = function (index) {
        var s = this._scores[index];
        if (!s && s !== 0)
            this._scores[index] = s = Procedure.average(this._samples[index]);
        return s;
    };
    Object.defineProperty(AlgebraFitness.prototype, "hasConverged", {
        get: function () {
            if (this._count < 10)
                return false;
            var len = this._scores.length;
            if (!len)
                return false;
            for (var i = 0; i < len; i++) {
                if (this.getScore(i) != 1)
                    return false;
            }
            return true;
        },
        enumerable: true,
        configurable: true
    });
    AlgebraFitness.prototype.compareTo = function (other) {
        for (var i = 0, len = this._scores.length; i < len; i++) {
            var a = this._scores[i], b = other.getScore(i);
            if (a < b || isNaN(a) && !isNaN(b))
                return -1;
            if (a > b || !isNaN(a) && isNaN(b))
                return +1;
            a = this._samples.length;
            b = other.count;
            if (a < b)
                return -1;
            if (a > b)
                return +1;
        }
        return 0;
    };
    return AlgebraFitness;
}());
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = AlgebraFitness;
//# sourceMappingURL=Fitness.js.map