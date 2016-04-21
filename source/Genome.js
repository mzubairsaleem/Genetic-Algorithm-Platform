/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Genome = (function () {
        function Genome() {
        }
        Object.defineProperty(Genome.prototype, "root", {
            get: function () {
                return this._root;
            },
            set: function (value) {
                this._hash = null;
                this._root = value;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Genome.prototype, "genes", {
            get: function () {
                var root = this._root;
                if (!root)
                    return [];
                return root.descendants.copyTo([root], 1);
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Genome.prototype, "hash", {
            get: function () {
                return this._hash || (this._hash = this.serialize());
            },
            enumerable: true,
            configurable: true
        });
        Genome.prototype.toString = function () {
            return this.hash;
        };
        return Genome;
    }());
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Genome;
});
//# sourceMappingURL=Genome.js.map