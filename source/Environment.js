(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports", "../node_modules/typescript-dotnet/source/System/Collections/LinkedList", "./Population"], factory);
    }
})(function (require, exports) {
    "use strict";
    var LinkedList_1 = require("../node_modules/typescript-dotnet/source/System/Collections/LinkedList");
    var Population_1 = require("./Population");
    var Environment = (function () {
        function Environment(_genomeFactory) {
            this._genomeFactory = _genomeFactory;
            this._problems = [];
            this._populations = new LinkedList_1.default();
        }
        Environment.prototype.test = function () {
            var _loop_1 = function(pr) {
                this_1._populations.forEach(function (po) { return pr.test(po); });
            };
            var this_1 = this;
            for (var _i = 0, _a = this._problems; _i < _a.length; _i++) {
                var pr = _a[_i];
                _loop_1(pr);
            }
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
    }());
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Environment;
});
//# sourceMappingURL=Environment.js.map