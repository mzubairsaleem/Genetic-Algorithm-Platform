/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
"use strict";
var Set_1 = require("typescript-dotnet-umd/System/Collections/Set");
var StringKeyDictionary_1 = require("typescript-dotnet-umd/System/Collections/Dictionaries/StringKeyDictionary");
var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
var ArgumentNullException_1 = require("typescript-dotnet-umd/System/Exceptions/ArgumentNullException");
var Enumerator_1 = require("typescript-dotnet-umd/System/Collections/Enumeration/Enumerator");
var Integer_1 = require("typescript-dotnet-umd/System/Integer");
var Population = (function () {
    function Population(_genomeFactory) {
        this._genomeFactory = _genomeFactory;
        this._population = new StringKeyDictionary_1.StringKeyDictionary();
    }
    Object.defineProperty(Population.prototype, "isReadOnly", {
        get: function () {
            return false;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Population.prototype, "count", {
        get: function () {
            return this._population.count;
        },
        enumerable: true,
        configurable: true
    });
    Population.prototype.remove = function (item) {
        var p = this._population;
        return item && p.removeByKey(item.hash) ? 1 : 0;
    };
    Population.prototype.clear = function () {
        return this._population.clear();
    };
    Population.prototype.contains = function (item) {
        var p = this._population;
        return !!item && p.containsKey(item.hash);
    };
    Population.prototype.copyTo = function (array, index) {
        if (index === void 0) { index = 0; }
        Integer_1.Integer.assertZeroOrGreater(index);
        if (!array)
            throw new ArgumentNullException_1.ArgumentNullException('array');
        var e = this._population.getEnumerator();
        while (e.moveNext()) {
            array[index++] = e.current.value;
        }
        return array;
    };
    Population.prototype.toArray = function () {
        return this.copyTo([]);
    };
    Population.prototype.forEach = function (action, useCopy) {
        return Enumerator_1.forEach(useCopy ? this.toArray() : this, action);
    };
    Population.prototype.getEnumerator = function () {
        return Linq_1.Enumerable
            .from(this._population)
            .select(function (o) { return o.value; })
            .getEnumerator();
    };
    Population.prototype.add = function (potential) {
        if (!potential) {
            var n = this._genomeFactory.generate();
            if (n)
                this.add(n);
        }
        else {
            var ts = void 0;
            var p = this._population;
            if (potential && !p.containsKey(ts = potential.hash)) {
                p.addByKeyValue(ts, potential);
            }
        }
    };
    Population.prototype.importEntries = function (genomes) {
        var _this = this;
        var imported = 0;
        Enumerator_1.forEach(genomes, function (o) {
            _this.add(o);
            imported++;
        });
        return imported;
    };
    Population.prototype.populate = function (count, rankedGenomes) {
        var f = this._genomeFactory;
        if (rankedGenomes && rankedGenomes.length) {
            var top_1 = rankedGenomes[0];
            if (!top_1.variationCountdown) {
                top_1.variationCountdown = 20;
                var v = f.generateVariations(top_1);
                console.log("Top Variations:", v.length);
                for (var _i = 0, v_1 = v; _i < v_1.length; _i++) {
                    var n = v_1[_i];
                    this.add(n);
                }
            }
            else {
                top_1.variationCountdown--;
            }
        }
        for (var i = 0; i < count; i++) {
            var n = f.generate(rankedGenomes);
            if (n)
                this.add(n);
        }
    };
    Population.prototype.keepOnly = function (selected) {
        var hashed = new Set_1.Set(Linq_1.Enumerable
            .from(selected)
            .select(function (o) { return o.hash; }));
        var p = this._population;
        p.forEach(function (o) {
            var key = o.key;
            if (!hashed.contains(key))
                p.removeByKey(key);
        }, true);
    };
    return Population;
}());
exports.Population = Population;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = Population;
//# sourceMappingURL=Population.js.map