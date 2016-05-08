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
        define(["require", "exports", "../node_modules/typescript-dotnet/source/System/Disposable/dispose", "../node_modules/typescript-dotnet/source/System/Collections/LinkedList", "../node_modules/typescript-dotnet/source/System/Tasks/TaskHandlerBase", "./Population", "../node_modules/typescript-dotnet/source/System.Linq/Linq", "./Triangular"], factory);
    }
})(function (require, exports) {
    "use strict";
    var dispose_1 = require("../node_modules/typescript-dotnet/source/System/Disposable/dispose");
    var LinkedList_1 = require("../node_modules/typescript-dotnet/source/System/Collections/LinkedList");
    var TaskHandlerBase_1 = require("../node_modules/typescript-dotnet/source/System/Tasks/TaskHandlerBase");
    var Population_1 = require("./Population");
    var Linq_1 = require("../node_modules/typescript-dotnet/source/System.Linq/Linq");
    var Triangular = require("./Triangular");
    var Environment = (function (_super) {
        __extends(Environment, _super);
        function Environment(_genomeFactory) {
            _super.call(this);
            this._genomeFactory = _genomeFactory;
            this.populationSize = 50;
            this.maxPopulations = 20;
            this.testCount = 10;
            this._problemsEnumerable
                = Linq_1.default.from(this._problems = []);
            this._populationsEnumerable
                = Linq_1.default.from(this._populations = new LinkedList_1.default());
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
        Object.defineProperty(Environment.prototype, "populations", {
            get: function () {
                return this._populations.count;
            },
            enumerable: true,
            configurable: true
        });
        Environment.prototype._onExecute = function () {
            var populations = this._populationsEnumerable.reverse(), problems = this._problemsEnumerable.memoize();
            var p = this.spawn(this.populationSize, Triangular.dispurse.decreasing(Linq_1.default.weave(populations
                .selectMany(function (o) { return problems.select(function (r) { return r.rank(o); }); }))));
            this.test();
            p.keepOnly(Linq_1.default.weave(problems.select(function (r) { return r.rank(p); }))
                .take(this.populationSize / 2));
            dispose_1.dispose(populations);
            this.execute(0);
        };
        Environment.prototype.spawn = function (populationSize, source) {
            var _ = this;
            var p = new Population_1.default(_._genomeFactory);
            p.populate(populationSize, Linq_1.default.from(source).toArray());
            _._populations.add(p);
            _._genomeFactory.trimPreviousGenomes();
            _.trimEarlyPopulations(_.maxPopulations);
            return p;
        };
        Environment.prototype.trimEarlyPopulations = function (maxPopulations) {
            var _this = this;
            var problems = this._problemsEnumerable.memoize();
            this._populationsEnumerable
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
    }(TaskHandlerBase_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Environment;
});
//# sourceMappingURL=Environment.js.map