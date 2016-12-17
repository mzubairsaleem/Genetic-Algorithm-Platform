/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
(function (dependencies, factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(dependencies, factory);
    }
})(["require", "exports", "typescript-dotnet-umd/System/Lazy", "typescript-dotnet-umd/System/Collections/List", "typescript-dotnet-umd/System/Exceptions/ArgumentException"], function (require, exports) {
    "use strict";
    var Lazy_1 = require("typescript-dotnet-umd/System/Lazy");
    var List_1 = require("typescript-dotnet-umd/System/Collections/List");
    var ArgumentException_1 = require("typescript-dotnet-umd/System/Exceptions/ArgumentException");
    var GeneBase = (function (_super) {
        __extends(GeneBase, _super);
        function GeneBase() {
            var _this = _super.call(this) || this;
            _this._readOnly = false;
            _this._onModified();
            return _this;
        }
        Object.defineProperty(GeneBase.prototype, "descendants", {
            get: function () {
                var d = this._descendants;
                if (!d) {
                    var e = this.linq;
                    d = e.concat(e.selectMany(function (s) { return s.descendants; }));
                    if (this.isReadOnly)
                        this._descendants = d;
                }
                return d;
            },
            enumerable: true,
            configurable: true
        });
        GeneBase.prototype.getIsReadOnly = function () {
            return this._readOnly;
        };
        GeneBase.prototype.setAsReadOnly = function () {
            if (!this._readOnly) {
                this._readOnly = true;
                this.forEach(function (c) { return c.setAsReadOnly(); });
            }
            return this;
        };
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
            this.throwIfDisposed();
            this.assertModifiable();
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
            this._descendants = void 0;
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
});
//# sourceMappingURL=GeneBase.js.map