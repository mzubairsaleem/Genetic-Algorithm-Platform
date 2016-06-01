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
var dispose_1 = require("typescript-dotnet/source/System/Disposable/dispose");
var LinkedList_1 = require("typescript-dotnet/source/System/Collections/LinkedList");
var TaskHandlerBase_1 = require("typescript-dotnet/source/System/Threading/Tasks/TaskHandlerBase");
var Population_1 = require("./Population");
var Linq_1 = require("typescript-dotnet/source/System.Linq/Linq");
var Environment = (function (_super) {
    __extends(Environment, _super);
    function Environment(_genomeFactory) {
        _super.call(this);
        this._genomeFactory = _genomeFactory;
        this._generations = 0;
        this.populationSize = 50;
        this.maxPopulations = 20;
        this.testCount = 10;
        this._problemsEnumerable
            = Linq_1.Enumerable.from(this._problems = []);
        this._populations = new LinkedList_1.LinkedList();
    }
    Environment.prototype.test = function (count) {
        if (count === void 0) { count = this.testCount; }
        var p = this._populations;
        var _loop_1 = function(pr) {
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
        var p = this.spawn(this.populationSize, Triangular.disperse.decreasing(Linq_1.Enumerable.weave(populations
            .selectMany(function (o) { return problems.select(function (r) { return r.rank(o); }); }))));
        this.test();
        this._generations++;
        p.keepOnly(Linq_1.Enumerable.weave(problems.select(function (r) { return r.rank(p); }))
            .take(this.populationSize / 2));
        dispose_1.dispose(populations);
    };
    Environment.prototype.spawn = function (populationSize, source) {
        var _ = this;
        var p = new Population_1.Population(_._genomeFactory);
        p.populate(populationSize, Linq_1.Enumerable.from(source).toArray());
        _._populations.add(p);
        _._genomeFactory.trimPreviousGenomes();
        _.trimEarlyPopulations(_.maxPopulations);
        return p;
    };
    Environment.prototype.trimEarlyPopulations = function (maxPopulations) {
        var _this = this;
        var problems = this._problemsEnumerable.memoize();
        this._populations.linq
            .takeExceptLast(maxPopulations)
            .forEach(function (p) {
            p.forEach(function (g) {
                if (problems.select(function (r) { return r.getFitnessFor(g).score; }).max() < 0.5)
                    p.remove(g);
            }, true);
            if (!p.count) {
                _this._populations.remove(p);
            }
        });
    };
    return Environment;
}(TaskHandlerBase_1.TaskHandlerBase));
exports.Environment = Environment;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = Environment;
//# sourceMappingURL=Environment.js.map