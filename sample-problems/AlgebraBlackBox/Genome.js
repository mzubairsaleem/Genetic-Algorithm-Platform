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
        define(["require", "exports", "../../source/Genome", "typescript-dotnet/source/System/Exceptions/InvalidOperationException"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Genome_1 = require("../../source/Genome");
    var InvalidOperationException_1 = require("typescript-dotnet/source/System/Exceptions/InvalidOperationException");
    var AlgebraGenome = (function (_super) {
        __extends(AlgebraGenome, _super);
        function AlgebraGenome(root) {
            _super.call(this, root);
        }
        AlgebraGenome.prototype.clone = function () {
            return new AlgebraGenome(this.root);
        };
        AlgebraGenome.prototype.serialize = function () {
            var root = this.root;
            if (!root)
                throw new InvalidOperationException_1.default("Cannot calculate a gene with no root.");
            return root.serialize();
        };
        AlgebraGenome.prototype.calculate = function (values) {
            var root = this.root;
            if (!root)
                throw new InvalidOperationException_1.default("Cannot calculate a gene with no root.");
            return root.calculate(values);
        };
        return AlgebraGenome;
    }(Genome_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = AlgebraGenome;
});
//# sourceMappingURL=Genome.js.map