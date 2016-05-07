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
        define(["require", "exports", "../node_modules/typescript-dotnet/source/System/Collections/LinkedList", "../node_modules/typescript-dotnet/source/System/Tasks/TaskHandlerBase", "./Population", "../node_modules/typescript-dotnet/source/System.Linq/Linq"], factory);
    }
})(function (require, exports) {
    "use strict";
    var LinkedList_1 = require("../node_modules/typescript-dotnet/source/System/Collections/LinkedList");
    var TaskHandlerBase_1 = require("../node_modules/typescript-dotnet/source/System/Tasks/TaskHandlerBase");
    var Population_1 = require("./Population");
    var Linq_1 = require("../node_modules/typescript-dotnet/source/System.Linq/Linq");
    var Environment = (function (_super) {
        __extends(Environment, _super);
        function Environment(_genomeFactory) {
            _super.call(this);
            this._genomeFactory = _genomeFactory;
            this.populationSize = 50;
            this.testCount = 10;
            this._problems = [];
            this._populations = new LinkedList_1.default();
        }
        Environment.prototype.test = function (count) {
            if (count === void 0) { count = this.testCount; }
            var _loop_1 = function(pr) {
                this_1._populations.forEach(function (po) { return pr.test(po, count); });
            };
            var this_1 = this;
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
            var populations = Linq_1.default.from(this._populations).reverse(), problems = Linq_1.default.from(this._problems).memoize();
            var p = this.spawn(this.populationSize, Linq_1.default.weave(populations
                .selectMany(function (o) { return problems.select(function (r) { return r.rank(o); }); })));
            this.test();
            p.keepOnly(Linq_1.default.weave(problems.select(function (r) { return r.rank(p); }))
                .take(this.populationSize / 2));
            this.execute(0);
        };
        Environment.prototype.spawn = function (populationSize, source) {
            var _ = this;
            var p = new Population_1.default(_._genomeFactory);
            source ? p.populateFrom(source, populationSize) : p.populate(populationSize);
            _._populations.add(p);
            _._genomeFactory.trimPreviousGenomes();
            _.trimEarlyPopulations(10);
            return p;
        };
        Environment.prototype.trimEarlyPopulations = function (maxPopulations) {
            var p = this._populations;
            while (p.count > maxPopulations) {
                p.removeFirst();
            }
        };
        return Environment;
    }(TaskHandlerBase_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Environment;
});
//# sourceMappingURL=Environment.js.map