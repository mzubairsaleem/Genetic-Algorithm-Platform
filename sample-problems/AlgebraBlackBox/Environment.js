"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Environment_1 = require("../../source/Environment");
var GenomeFactory_1 = require("./GenomeFactory");
var Problem_1 = require("./Problem");
var Linq_1 = require("typescript-dotnet/source/System.Linq/Linq");
var Utility_1 = require("typescript-dotnet/source/System/Text/Utility");
function actualFormula(a, b) {
    return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2));
}
var VARIABLE_NAMES = Linq_1.Enumerable.from("abcdefghijklmnopqrstuvwxyz").toArray();
function convertParameterToAlphabet(source) {
    return Utility_1.supplant(source, VARIABLE_NAMES);
}
exports.convertParameterToAlphabet = convertParameterToAlphabet;
var AlgebraEnvironmentSample = (function (_super) {
    __extends(AlgebraEnvironmentSample, _super);
    function AlgebraEnvironmentSample() {
        _super.call(this, new GenomeFactory_1.default());
        this._problems
            .push(new Problem_1.default(actualFormula));
    }
    AlgebraEnvironmentSample.prototype._onExecute = function () {
        _super.prototype._onExecute.call(this);
        console.log("Generation:", this._generations);
        var problems = Linq_1.Enumerable.from(this._problems).memoize();
        var p = this._populations.linq.selectMany(function (s) { return s; });
        var top = Linq_1.Enumerable
            .weave(problems
            .select(function (r) {
            return Linq_1.Enumerable.from(r.rank(p))
                .select(function (g) {
                var red = g.root.asReduced(), suffix = "";
                if (red != g.root)
                    suffix = " => " + convertParameterToAlphabet(red.toString());
                return {
                    label: r.getFitnessFor(g).score + ": " + convertParameterToAlphabet(g.hash) + suffix,
                    gene: g
                };
            });
        }))
            .take(this._problems.length)
            .memoize();
        var n = this._populations.last.value;
        n.importEntries(top
            .select(function (g) { return g.gene; })
            .where(function (g) { return g.root.isReducible() && g.root.asReduced() != g.root; })
            .select(function (g) {
            var n = g.clone();
            n.root = g.root.asReduced();
            return n;
        }));
        console.log("Population Size:", n.count);
        var c = problems.selectMany(function (p) { return p.convergent; }).count();
        if (c)
            console.log("Convergent:", c);
        console.log("Top:", top.select(function (s) { return s.label; }).toArray(), "\n");
        this.start();
    };
    return AlgebraEnvironmentSample;
}(Environment_1.default));
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = AlgebraEnvironmentSample;
//# sourceMappingURL=Environment.js.map