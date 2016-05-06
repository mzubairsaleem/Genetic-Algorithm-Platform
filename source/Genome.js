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
        function Genome(_root) {
            this._root = _root;
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
        Genome.prototype.findParent = function (child) {
            var root = this.root;
            return (root && child != root)
                ? root.findParent(child)
                : null;
        };
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
        Genome.prototype.equals = function (other) {
            return this == other || this.root == other.root || this.hash === other.hash;
        };
        return Genome;
    }());
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = Genome;
});
//# sourceMappingURL=Genome.js.map