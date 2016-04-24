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
        define(["require", "exports", "../../source/Genome", "../../node_modules/typescript-dotnet/source/System/Exceptions/NotImplementedException"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Genome_1 = require("../../source/Genome");
    var NotImplementedException_1 = require("../../node_modules/typescript-dotnet/source/System/Exceptions/NotImplementedException");
    var AlgebraGenome = (function (_super) {
        __extends(AlgebraGenome, _super);
        function AlgebraGenome() {
            _super.apply(this, arguments);
        }
        AlgebraGenome.prototype.clone = function () {
            throw new NotImplementedException_1.default();
        };
        AlgebraGenome.prototype.compareTo = function (other) {
            throw new NotImplementedException_1.default();
        };
        AlgebraGenome.prototype.serialize = function () {
            throw new NotImplementedException_1.default();
        };
        AlgebraGenome.prototype.calculate = function (values) {
            throw new NotImplementedException_1.default();
        };
        return AlgebraGenome;
    }(Genome_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraGenome;
});
//# sourceMappingURL=Genome.js.map