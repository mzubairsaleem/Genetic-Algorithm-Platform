/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
"use strict";
var Linq_1 = require("typescript-dotnet-umd/System.Linq/Linq");
var Genome = (function () {
    function Genome(_root) {
        this._root = _root;
        this._hash = null;
    }
    Object.defineProperty(Genome.prototype, "root", {
        get: function () {
            return this._root;
        },
        set: function (value) {
            if (value != this._root) {
                this.resetHash();
                this._root = value;
            }
        },
        enumerable: true,
        configurable: true
    });
    Genome.prototype.findParent = function (child) {
        return this.root.findParent(child);
    };
    Object.defineProperty(Genome.prototype, "genes", {
        get: function () {
            var root = this.root;
            return Linq_1.Enumerable
                .make(root)
                .concat(root.descendants);
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
    Genome.prototype.resetHash = function () {
        this._hash = null;
        if (this._root)
            this._root.resetToString();
    };
    Genome.prototype.toString = function () {
        return this.hash;
    };
    Genome.prototype.equals = function (other) {
        return this == other || this.root == other.root || this.hash === other.hash;
    };
    return Genome;
}());
exports.Genome = Genome;
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = Genome;
//# sourceMappingURL=Genome.js.map