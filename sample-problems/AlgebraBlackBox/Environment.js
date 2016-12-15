var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments)).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t;
    return { next: verb(0), "throw": verb(1), "return": verb(2) };
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
(function (dependencies, factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(dependencies, factory);
    }
})(["require", "exports", "../../source/Environment", "./GenomeFactory", "./Problem", "typescript-dotnet-umd/System.Linq/Linq", "typescript-dotnet-umd/System/Promises/Promise"], function (require, exports) {
    "use strict";
    var Environment_1 = require("../../source/Environment");
    var GenomeFactory_1 = require("./GenomeFactory");
    var Problem_1 = require("./Problem");
    var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
    var Promise_1 = require("typescript-dotnet-umd/System/Promises/Promise");
    function actualFormula(a, b) {
        return Math.sqrt(Math.pow(a, 2) + Math.pow(b, 2) + a) + b;
    }
    var AlgebraEnvironmentSample = (function (_super) {
        __extends(AlgebraEnvironmentSample, _super);
        function AlgebraEnvironmentSample() {
            var _this = _super.call(this, new GenomeFactory_1.default()) || this;
            _this._problems
                .push(new Problem_1.default(actualFormula));
            return _this;
        }
        AlgebraEnvironmentSample.prototype._onAsyncExecute = function () {
            return __awaiter(this, void 0, Promise_1.Promise, function () {
                var problems, p_1, top_1, c, topOutput, n, ex_1;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            _a.trys.push([0, 2, , 3]);
                            return [4 /*yield*/, _super.prototype._onAsyncExecute.call(this)];
                        case 1:
                            _a.sent();
                            problems = Linq_1.Enumerable(this._problems).memoize();
                            p_1 = this._populations.linq
                                .selectMany(function (s) { return s; })
                                .orderBy(function (g) { return g.hash.length; })
                                .groupBy(function (g) { return g.hashReduced; })
                                .select(function (g) { return g.first(); });
                            top_1 = Linq_1.Enumerable
                                .weave(problems
                                .select(function (r) {
                                return Linq_1.Enumerable(r.rank(p_1))
                                    .select(function (g) {
                                    var red = g.root.asReduced(), suffix = "";
                                    if (red != g.root)
                                        suffix
                                            = " => " + g.toAlphaParameters(true);
                                    var f = r.getFitnessFor(g);
                                    return {
                                        label: "" + g.toAlphaParameters() + suffix + ": (" + f.count + " samples) " + f.scores,
                                        gene: g
                                    };
                                });
                            }))
                                .take(this._problems.length)
                                .memoize();
                            c = problems.selectMany(function (p) { return p.convergent; }).toArray();
                            topOutput = "\n\t" + top_1.select(function (s) { return s.label; }).toArray().join("\n\t");
                            this.state = "Top Genome: " + topOutput.replace(": ", ":\n\t");
                            console.log("Top:", topOutput);
                            if (c.length)
                                console.log("\nConvergent:", c.map(function (g) { return g.toAlphaParameters(true); }));
                            if (problems.count(function (p) { return p.convergent.length != 0; }) < this._problems.length) {
                                n = this._populations.last.value;
                                n.importEntries(top_1
                                    .select(function (g) { return g.gene; })
                                    .where(function (g) { return g.root.isReducible() && g.root.asReduced() != g.root; })
                                    .select(function (g) {
                                    var n = g.clone();
                                    n.root = g.root.asReduced();
                                    return n;
                                }));
                                this.start();
                            }
                            console.log("");
                            return [3 /*break*/, 3];
                        case 2:
                            ex_1 = _a.sent();
                            console.error(ex_1, ex_1.stack);
                            return [3 /*break*/, 3];
                        case 3: return [2 /*return*/];
                    }
                });
            });
        };
        return AlgebraEnvironmentSample;
    }(Environment_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraEnvironmentSample;
});
//# sourceMappingURL=Environment.js.map