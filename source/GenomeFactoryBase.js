/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
"use strict";
var OrderedStringKeyDictionary_1 = require("typescript-dotnet-umd/System/Collections/Dictionaries/OrderedStringKeyDictionary");
var GenomeFactoryBase = (function () {
    function GenomeFactoryBase(maxGenomeTracking) {
        if (maxGenomeTracking === void 0) { maxGenomeTracking = 10000; }
        this.maxGenomeTracking = maxGenomeTracking;
        this._previousGenomes = new OrderedStringKeyDictionary_1.OrderedStringKeyDictionary();
    }
    Object.defineProperty(GenomeFactoryBase.prototype, "previousGenomes", {
        get: function () {
            return this._previousGenomes.keys;
        },
        enumerable: true,
        configurable: true
    });
    GenomeFactoryBase.prototype.getPrevious = function (hash) {
        return this._previousGenomes.getValue(hash);
    };
    GenomeFactoryBase.prototype.trimPreviousGenomes = function () {
        while (this._previousGenomes.count > this.maxGenomeTracking) {
            this._previousGenomes.removeByIndex(0);
        }
    };
    return GenomeFactoryBase;
}());
exports.GenomeFactoryBase = GenomeFactoryBase;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = GenomeFactoryBase;
//# sourceMappingURL=GenomeFactoryBase.js.map