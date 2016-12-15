(function (dependencies, factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(dependencies, factory);
    }
})(["require", "exports", "typescript-dotnet-umd/System/Collections/Array/Procedure"], function (require, exports) {
    "use strict";
    var Procedure = require("typescript-dotnet-umd/System/Collections/Array/Procedure");
    var Fitness = (function () {
        function Fitness() {
            this._scoreCard = [];
            this._scores = [];
            this._count = 0;
        }
        Object.defineProperty(Fitness.prototype, "count", {
            get: function () {
                return this._count;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Fitness.prototype, "scores", {
            get: function () {
                var _this = this;
                return this._scores.map(function (v, i) { return _this.getScore(i); });
            },
            enumerable: true,
            configurable: true
        });
        Fitness.prototype.add = function (score) {
            if (!score || !score.length)
                return;
            for (var i = 0, len = score.length; i < len; i++) {
                var s = this._scoreCard[i];
                if (!s)
                    this._scoreCard[i] = s = [];
                s.push(score[i]);
                this._scores[i] = null;
            }
            this._count++;
        };
        Fitness.prototype.getScore = function (index) {
            var s = this._scores[index];
            if (!s && s !== 0)
                this._scores[index] = s = Procedure.average(this._scoreCard[index]);
            return s;
        };
        Fitness.prototype.hasConverged = function (minSamples) {
            if (minSamples === void 0) { minSamples = 100; }
            if (this._count < minSamples)
                return false;
            var len = this._scores.length;
            if (!len)
                return false;
            for (var i = 0; i < len; i++) {
                if (this.getScore(i) != 1)
                    return false;
            }
            return true;
        };
        Fitness.prototype.compareTo = function (other) {
            for (var i = 0, len = this._scores.length; i < len; i++) {
                var a_1 = this._scores[i], b_1 = other.getScore(i);
                if (a_1 < b_1 || isNaN(a_1) && !isNaN(b_1))
                    return -1;
                if (a_1 > b_1 || !isNaN(a_1) && isNaN(b_1))
                    return +1;
            }
            var a = this._count, b = other.count;
            if (a < b)
                return -1;
            if (a > b)
                return +1;
            return 0;
        };
        return Fitness;
    }());
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Fitness;
});
//# sourceMappingURL=Fitness.js.map