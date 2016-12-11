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
var Lazy_1 = require("typescript-dotnet-umd/System/Lazy");
var List_1 = require("typescript-dotnet-umd/System/Collections/List");
var ArgumentException_1 = require("typescript-dotnet-umd/System/Exceptions/ArgumentException");
var GeneBase = (function (_super) {
    __extends(GeneBase, _super);
    function GeneBase() {
        var _this = _super.call(this) || this;
        _this.resetToString();
        return _this;
    }
    Object.defineProperty(GeneBase.prototype, "descendants", {
        get: function () {
            var e = this.linq;
            return e.concat(e.selectMany(function (s) { return s.descendants; }));
        },
        enumerable: true,
        configurable: true
    });
    GeneBase.prototype.findParent = function (child) {
        var children = this._source;
        if (!children || !children.length)
            return null;
        if (children.indexOf(child) != -1)
            return this;
        for (var _i = 0, children_1 = children; _i < children_1.length; _i++) {
            var c = children_1[_i];
            var p = c.findParent(child);
            if (p)
                return p;
        }
        return null;
    };
    GeneBase.prototype._replaceInternal = function (target, replacement, throwIfNotFound) {
        var s = this._source;
        var index = this._source.indexOf(target);
        if (index == -1) {
            if (throwIfNotFound)
                throw new ArgumentException_1.ArgumentException('target', "gene not found.");
            return false;
        }
        s[index] = replacement;
        return true;
    };
    GeneBase.prototype.replace = function (target, replacement, throwIfNotFound) {
        var m = this._replaceInternal(target, replacement, throwIfNotFound);
        if (m)
            this._onModified();
        return m;
    };
    GeneBase.prototype.resetToString = function () {
        var _this = this;
        var ts = this._toString;
        if (ts)
            ts.tryReset();
        else
            this._toString = new Lazy_1.Lazy(function () { return _this.toStringInternal(); }, false, true);
        this.forEach(function (c) { return c.resetToString(); });
    };
    GeneBase.prototype._onModified = function () {
        _super.prototype._onModified.call(this);
        this.resetToString();
    };
    GeneBase.prototype.toString = function () {
        return this._toString.value;
    };
    GeneBase.prototype.equals = function (other) {
        return this === other || this.toString() == other.toString();
    };
    return GeneBase;
}(List_1.List));
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = GeneBase;
//# sourceMappingURL=GeneBase.js.map