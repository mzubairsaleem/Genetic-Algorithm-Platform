"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Environment_1 = require("../../source/Environment");
var GenomeFactory_1 = require("./GenomeFactory");
var Problem_1 = require("./Problem");
var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
var Utility_1 = require("typescript-dotnet-umd/System/Text/Utility");
function actualFormula(a, b) {
    return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
}
var VARIABLE_NAMES = Object.freeze(Linq_1.Enumerable("abcdefghijklmnopqrstuvwxyz").toArray());
function convertParameterToAlphabet(source) {
    return Utility_1.supplant(source, VARIABLE_NAMES);
}
exports.convertParameterToAlphabet = convertParameterToAlphabet;
var AlgebraEnvironmentSample = (function (_super) {
    __extends(AlgebraEnvironmentSample, _super);
    function AlgebraEnvironmentSample() {
        var _this = _super.call(this, new GenomeFactory_1.default()) || this;
        _this._problems
            .push(new Problem_1.default(actualFormula));
        return _this;
    }
    AlgebraEnvironmentSample.prototype._onExecute = function () {
        try {
            _super.prototype._onExecute.call(this);
            var problems = Linq_1.Enumerable(this._problems).memoize();
            var p_1 = this._populations.linq
                .selectMany(function (s) { return s; })
                .orderBy(function (g) { return g.hash.length; })
                .groupBy(function (g) { return g.hashReduced; })
                .select(function (g) { return g.first(); });
            var top_1 = Linq_1.Enumerable
                .weave(problems
                .select(function (r) {
                return Linq_1.Enumerable(r.rank(p_1))
                    .select(function (g) {
                    var red = g.root.asReduced(), suffix = "";
                    if (red != g.root)
                        suffix
                            = " => " + convertParameterToAlphabet(red.toString());
                    var f = r.getFitnessFor(g);
                    return {
                        label: "(" + f.count + ") " + f.scores + ": " + convertParameterToAlphabet(g.hash) + suffix,
                        gene: g
                    };
                });
            }))
                .take(this._problems.length)
                .memoize();
            var c = problems.selectMany(function (p) { return p.convergent; }).toArray();
            console.log("Top:", top_1.select(function (s) { return s.label; }).toArray(), "\n");
            if (c.length)
                console.log("\nConvergent:", c.map(function (g) { return convertParameterToAlphabet(g.hashReduced); }));
            if (problems.count(function (p) { return p.convergent.length != 0; }) < this._problems.length) {
                var n = this._populations.last.value;
                n.importEntries(top_1
                    .select(function (g) { return g.gene; })
                    .where(function (g) { return g.root.isReducible() && g.root.asReduced() != g.root; })
                    .select(function (g) {
                    var n = g.clone();
                    n.root = g.root.asReduced();
                    return n;
                }));
                console.log("Population Size:", n.count);
                this.start();
            }
        }
        catch (ex) {
            console.error(ex, ex.stack);
        }
    };
    return AlgebraEnvironmentSample;
}(Environment_1.default));
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = AlgebraEnvironmentSample;
//# sourceMappingURL=Environment.js.map