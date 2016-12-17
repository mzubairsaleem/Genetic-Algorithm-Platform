var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
(function (dependencies, factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(dependencies, factory);
    }
})(["require", "exports", "typescript-dotnet-umd/System/Collections/Array/Procedure", "typescript-dotnet-umd/System/Collections/List", "typescript-dotnet-umd/System/Exception"], function (require, exports) {
    "use strict";
    var Procedure = require("typescript-dotnet-umd/System/Collections/Array/Procedure");
    var List_1 = require("typescript-dotnet-umd/System/Collections/List");
    var Exception_1 = require("typescript-dotnet-umd/System/Exception");
    var SingularFitness = (function (_super) {
        __extends(SingularFitness, _super);
        function SingularFitness() {
            return _super.call(this) || this;
        }
        SingularFitness.prototype.add = function (entry) {
            this._average = null;
            _super.prototype.add.call(this, entry);
        };
        Object.defineProperty(SingularFitness.prototype, "average", {
            get: function () {
                var v = this._average;
                if (v == null)
                    this._average = v = Procedure.average(this._source);
                return v;
            },
            enumerable: true,
            configurable: true
        });
        return SingularFitness;
    }(List_1.List));
    exports.SingularFitness = SingularFitness;
    var Fitness = (function (_super) {
        __extends(Fitness, _super);
        function Fitness() {
            return _super.apply(this, arguments) || this;
        }
        Fitness.prototype.addTheseScores = function (scores) {
            var len = scores.length, count = this.count;
            for (var i = 0; i < len; i++) {
                var f = void 0;
                if (i >= count)
                    this.set(i, f = new SingularFitness());
                else
                    f = this.get(i);
                f.add(scores[i]);
            }
        };
        Fitness.prototype.addScores = function () {
            var scores = [];
            for (var _i = 0; _i < arguments.length; _i++) {
                scores[_i] = arguments[_i];
            }
            this.addTheseScores(scores);
        };
        Object.defineProperty(Fitness.prototype, "sampleCount", {
            get: function () {
                if (!this.count)
                    return 0;
                return this.linq.select(function (s) { return s.count; }).min();
            },
            enumerable: true,
            configurable: true
        });
        Fitness.prototype.hasConverged = function (minSamples, convergence, tolerance) {
            if (minSamples === void 0) { minSamples = 100; }
            if (convergence === void 0) { convergence = 1; }
            if (tolerance === void 0) { tolerance = 0; }
            if (minSamples > this.sampleCount)
                return false;
            for (var _i = 0, _a = this._source; _i < _a.length; _i++) {
                var s = _a[_i];
                var score = s.average;
                if (score > convergence)
                    throw new Exception_1.Exception("Score has exceeded convergence value.");
                if (score < convergence - tolerance)
                    return false;
            }
            return true;
        };
        Fitness.prototype.compareTo = function (other) {
            var len = this._source.length;
            for (var i = 0; i < len; i++) {
                var a = this.get(i);
                var b = other.get(i);
                var aA = a.average;
                var bA = b.average;
                if (aA < bA || isNaN(aA) && !isNaN(bA))
                    return -1;
                if (aA > bA || !isNaN(aA) && isNaN(bA))
                    return +1;
                if (a.count < b.count)
                    return -1;
                if (a.count > b.count)
                    return +1;
            }
            return 0;
        };
        Object.defineProperty(Fitness.prototype, "scores", {
            get: function () {
                return this._source.map(function (s) { return s.average; });
            },
            enumerable: true,
            configurable: true
        });
        return Fitness;
    }(List_1.List));
    exports.Fitness = Fitness;
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Fitness;
});
//# sourceMappingURL=Fitness.js.map