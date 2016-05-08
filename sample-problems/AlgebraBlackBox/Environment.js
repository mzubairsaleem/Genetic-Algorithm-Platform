var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports", "../../source/Environment", "./GenomeFactory", "./Problem", "../../node_modules/typescript-dotnet/source/System.Linq/Linq"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Environment_1 = require("../../source/Environment");
    var GenomeFactory_1 = require("./GenomeFactory");
    var Problem_1 = require("./Problem");
    var Linq_1 = require("../../node_modules/typescript-dotnet/source/System.Linq/Linq");
    function actualFormula(a, b) {
        return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
    }
    var AlgebraEnvironmentSample = (function (_super) {
        __extends(AlgebraEnvironmentSample, _super);
        function AlgebraEnvironmentSample() {
            _super.call(this, new GenomeFactory_1.default());
            this._problems
                .push(new Problem_1.default(actualFormula));
        }
        AlgebraEnvironmentSample.prototype._onExecute = function () {
            console.log("Executing...");
            _super.prototype._onExecute.call(this);
            console.log("super._onExecute() complete.");
            var problems = Linq_1.default.from(this._problems).memoize();
            var p = Linq_1.default.from(this._populations).selectMany(function (s) { return s; });
            var top = Linq_1.default
                .weave(problems
                .select(function (r) {
                return Linq_1.default.from(r.rank(p))
                    .select(function (g) { return g.hash + ": " + r.getFitnessFor(g).score; });
            }))
                .take(this._problems.length).toArray();
            console.log("Top:", top);
        };
        return AlgebraEnvironmentSample;
    }(Environment_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraEnvironmentSample;
});
//# sourceMappingURL=Environment.js.map