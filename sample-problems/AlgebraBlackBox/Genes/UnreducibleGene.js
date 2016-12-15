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
})(["require", "exports", "../Gene"], function (require, exports) {
    "use strict";
    var Gene_1 = require("../Gene");
    var UnreducibleGene = (function (_super) {
        __extends(UnreducibleGene, _super);
        function UnreducibleGene() {
            return _super.apply(this, arguments) || this;
        }
        UnreducibleGene.prototype.isReducible = function () {
            return false;
        };
        UnreducibleGene.prototype.asReduced = function () {
            return this;
        };
        return UnreducibleGene;
    }(Gene_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = UnreducibleGene;
});
//# sourceMappingURL=UnreducibleGene.js.map