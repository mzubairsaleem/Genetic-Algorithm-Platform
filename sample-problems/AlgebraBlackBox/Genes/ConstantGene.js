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
        define(["require", "exports", "../Gene"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Gene_1 = require("../Gene");
    var EMPTY = "";
    var ConstantGene = (function (_super) {
        __extends(ConstantGene, _super);
        function ConstantGene() {
            _super.apply(this, arguments);
        }
        ConstantGene.prototype.toStringInternal = function () {
            return this.multiple + EMPTY;
        };
        ConstantGene.prototype.toStringContents = function () {
            return EMPTY;
        };
        ConstantGene.prototype.clone = function () {
            return new ConstantGene(this.multiple);
        };
        ConstantGene.prototype.calculateWithoutMultiple = function (values) {
            return 1;
        };
        ConstantGene.prototype.asReduced = function () {
            return this.clone();
        };
        ConstantGene.prototype.equals = function (other) {
            return this.multiple == other.multiple;
        };
        return ConstantGene;
    }(Gene_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = ConstantGene;
});
//# sourceMappingURL=ConstantGene.js.map