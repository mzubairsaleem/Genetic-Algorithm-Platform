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
        define(["require", "exports", "../../source/GeneBase"], factory);
    }
})(function (require, exports) {
    "use strict";
    var GeneBase_1 = require("../../source/GeneBase");
    var EMPTY = "";
    var AlgebraGene = (function (_super) {
        __extends(AlgebraGene, _super);
        function AlgebraGene(_multiple) {
            if (_multiple === void 0) { _multiple = 1; }
            _super.call(this);
            this._multiple = _multiple;
        }
        AlgebraGene.prototype.serialize = function () {
            return this.toString();
        };
        Object.defineProperty(AlgebraGene.prototype, "multiple", {
            get: function () {
                return this._multiple;
            },
            set: function (value) {
                this._multiple = value;
                this._onModified();
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(AlgebraGene.prototype, "multiplePrefix", {
            get: function () {
                var m = this._multiple;
                if (m != 1)
                    return m == -1 ? "-" : (m + EMPTY);
                return EMPTY;
            },
            enumerable: true,
            configurable: true
        });
        AlgebraGene.prototype.toStringInternal = function () {
            return this.multiplePrefix
                + this.toStringContents();
        };
        AlgebraGene.prototype.calculate = function (values) {
            return this._multiple
                * this.calculateWithoutMultiple(values);
        };
        return AlgebraGene;
    }(GeneBase_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraGene;
});
//# sourceMappingURL=Gene.js.map