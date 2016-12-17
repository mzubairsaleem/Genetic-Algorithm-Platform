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
})(["require", "exports", "./Triangular", "typescript-dotnet-umd/System/Disposable/dispose", "typescript-dotnet-umd/System/Collections/LinkedList", "typescript-dotnet-umd/System/Threading/Tasks/TaskHandlerBase", "./Population", "typescript-dotnet-umd/System.Linq/Linq", "typescript-dotnet-umd/System/Promises/Promise", "typescript-dotnet-umd/System/Diagnostics/Stopwatch"], function (require, exports) {
    "use strict";
    var Triangular = require("./Triangular");
    var dispose_1 = require("typescript-dotnet-umd/System/Disposable/dispose");
    var LinkedList_1 = require("typescript-dotnet-umd/System/Collections/LinkedList");
    var TaskHandlerBase_1 = require("typescript-dotnet-umd/System/Threading/Tasks/TaskHandlerBase");
    var Population_1 = require("./Population");
    var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
    var Promise_1 = require("typescript-dotnet-umd/System/Promises/Promise");
    var Stopwatch_1 = require("typescript-dotnet-umd/System/Diagnostics/Stopwatch");
    var Environment = (function (_super) {
        __extends(Environment, _super);
        function Environment(_genomeFactory) {
            var _this = _super.call(this) || this;
            _this._genomeFactory = _genomeFactory;
            _this._generations = 0;
            _this.populationSize = 50;
            _this.maxPopulations = 10;
            _this.testCount = 5;
            _this._totalTime = 0;
            _this._problemsEnumerable
                = Linq_1.Enumerable(_this._problems = []);
            _this._populations = new LinkedList_1.LinkedList();
            return _this;
        }
        Environment.prototype.test = function (count) {
            if (count === void 0) { count = this.testCount; }
            return __awaiter(this, void 0, Promise_1.Promise, function () {
                var p, results, result;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            p = this._populations.toArray();
                            results = this._problems.map(function (problem) {
                                var calcP = p.map(function (population) { return problem.test(population, count); });
                                var a = Promise_1.Promise.all(calcP);
                                a.delayAfterResolve(10).then(function () { return dispose_1.dispose.these(calcP); });
                                return a;
                            });
                            result = Promise_1.Promise.all(results);
                            return [4 /*yield*/, result];
                        case 1:
                            _a.sent();
                            dispose_1.dispose.these(results);
                            return [2 /*return*/];
                    }
                });
            });
        };
        Object.defineProperty(Environment.prototype, "generations", {
            get: function () {
                return this._generations;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Environment.prototype, "populations", {
            get: function () {
                return this._populations.count;
            },
            enumerable: true,
            configurable: true
        });
        Environment.prototype._onAsyncExecute = function () {
            return __awaiter(this, void 0, Promise_1.Promise, function () {
                var sw, populations, problems, allGenes, previousP, p, beforeCulling, additional, time;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            sw = Stopwatch_1.default.startNew();
                            populations = this._populations.linq.reverse(), problems = this._problemsEnumerable.memoize();
                            sw.lap();
                            allGenes = populations.selectMany(function (g) { return g; }).memoize();
                            previousP = problems.select(function (r) { return r.rank(allGenes); });
                            p = this.spawn(this.populationSize, previousP.any() ?
                                Triangular.disperse.decreasing(Linq_1.Enumerable.weave(previousP)) : void 0);
                            beforeCulling = p.count;
                            if (!beforeCulling)
                                throw "Nothing spawned!!!";
                            console.log("Populations:", this._populations.count);
                            console.log("Selection/Ranking (ms):", sw.currentLapMilliseconds);
                            sw.lap();
                            return [4 /*yield*/, this.test()];
                        case 1:
                            _a.sent();
                            this._generations++;
                            additional = Math.max(p.count - this.populationSize, 0);
                            p.keepOnly(Linq_1.Enumerable.weave(problems.select(function (r) { return r.rank(p); }))
                                .take(this.populationSize / 2 + additional));
                            console.log("Population Size:", p.count, '/', beforeCulling);
                            dispose_1.dispose(populations);
                            console.log("Testing/Cleanup (ms):", sw.currentLapMilliseconds);
                            time = sw.elapsedMilliseconds;
                            this._totalTime += time;
                            console.log("Generations:", this._generations + ",", "Time:", time, "current /", this._totalTime, "total", "(" + Math.floor(this._totalTime / this._generations), "average)");
                            return [2 /*return*/];
                    }
                });
            });
        };
        Environment.prototype._onExecute = function () {
            this._onAsyncExecute();
        };
        Environment.prototype.spawn = function (populationSize, source) {
            var _ = this;
            var p = new Population_1.Population(_._genomeFactory);
            p.populate(populationSize, source && Linq_1.Enumerable(source).toArray());
            _._populations.add(p);
            _._genomeFactory.trimPreviousGenomes();
            _.trimEarlyPopulations(_.maxPopulations);
            return p;
        };
        Environment.prototype.getNewPopulation = function () {
            return new Population_1.Population(this._genomeFactory);
        };
        Environment.prototype.trimEarlyPopulations = function (maxPopulations) {
            var problems = this._problemsEnumerable.memoize(), pops = this._populations;
            pops.linq
                .takeExceptLast(maxPopulations)
                .forEach(function (p) {
                problems.forEach(function (r) {
                    var keep = Linq_1.Enumerable(r.rank(p)).firstOrDefault();
                    if (keep)
                        pops.last.value.add(keep);
                });
                pops.remove(p);
            });
        };
        return Environment;
    }(TaskHandlerBase_1.TaskHandlerBase));
    exports.Environment = Environment;
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Environment;
});
//# sourceMappingURL=Environment.js.map