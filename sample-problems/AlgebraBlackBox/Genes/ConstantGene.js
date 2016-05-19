/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
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
        define(["require", "exports", "./UnreducibleGene"], factory);
    }
})(function (require, exports) {
    "use strict";
    var UnreducibleGene_1 = require("./UnreducibleGene");
    var EMPTY = "";
    var ConstantGene = (function (_super) {
        __extends(ConstantGene, _super);
        function ConstantGene() {
            _super.apply(this, arguments);
        }
        ConstantGene.prototype.toStringInternal = function () {
            return this._multiple + EMPTY;
        };
        ConstantGene.prototype.toStringContents = function () {
            return EMPTY;
        };
        ConstantGene.prototype.clone = function () {
            return new ConstantGene(this._multiple);
        };
        ConstantGene.prototype.calculateWithoutMultiple = function (values) {
            return 1;
        };
        ConstantGene.prototype.equals = function (other) {
            return other instanceof ConstantGene && this._multiple == other._multiple || _super.prototype.equals.call(this, other);
        };
        return ConstantGene;
    }(UnreducibleGene_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = ConstantGene;
});
//# sourceMappingURL=ConstantGene.js.map