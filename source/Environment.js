/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Triangular = require("./Triangular");
var dispose_1 = require("typescript-dotnet-umd/System/Disposable/dispose");
var LinkedList_1 = require("typescript-dotnet-umd/System/Collections/LinkedList");
var TaskHandlerBase_1 = require("typescript-dotnet-umd/System/Threading/Tasks/TaskHandlerBase");
var Population_1 = require("./Population");
var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
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
        _this._problemsEnumerable
            = Linq_1.Enumerable(_this._problems = []);
        _this._populations = new LinkedList_1.LinkedList();
        return _this;
    }
    Environment.prototype.test = function (count) {
        if (count === void 0) { count = this.testCount; }
        var p = this._populations;
        var _loop_1 = function (pr) {
            p.forEach(function (po) { return pr.test(po, count); });
        };
        for (var _i = 0, _a = this._problems; _i < _a.length; _i++) {
            var pr = _a[_i];
            _loop_1(pr);
        }
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
    Environment.prototype._onExecute = function () {
        var populations = this._populations.linq.reverse(), problems = this._problemsEnumerable.memoize();
        var sw = Stopwatch_1.default.startNew();
        var previousP = populations
            .selectMany(function (o) {
            var x = problems.select(function (r) { return r.rank(o); });
            if (!x.any())
                return x;
            return Linq_1.Enumerable.make(x.first()).concat(x);
        }).memoize();
        var p = this.spawn(this.populationSize, previousP.any() ?
            Triangular.disperse.decreasing(Linq_1.Enumerable.weave(previousP)) : void 0);
        if (!p.count)
            throw "Nothing spawned!!!";
        console.log("Populations:", this._populations.count);
        console.log("Selection/Ranking (ms):", sw.currentLapMilliseconds);
        sw.lap();
        this.test();
        this._generations++;
        p.keepOnly(Linq_1.Enumerable.weave(problems.select(function (r) { return r.rank(p); }))
            .take(this.populationSize / 2));
        dispose_1.dispose(populations);
        console.log("Testing/Cleanup (ms):", sw.currentLapMilliseconds);
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
//# sourceMappingURL=Environment.js.map