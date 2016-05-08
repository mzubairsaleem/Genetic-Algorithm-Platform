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
        define(["require", "exports", "../../../node_modules/typescript-dotnet/source/System/Integer", "../Gene"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Integer_1 = require("../../../node_modules/typescript-dotnet/source/System/Integer");
    var Gene_1 = require("../Gene");
    var ParameterGene = (function (_super) {
        __extends(ParameterGene, _super);
        function ParameterGene(_id, multiple) {
            if (multiple === void 0) { multiple = 1; }
            _super.call(this, multiple);
            this._id = _id;
            Integer_1.default.assert(_id, 'id');
        }
        Object.defineProperty(ParameterGene.prototype, "id", {
            get: function () {
                return this._id;
            },
            enumerable: true,
            configurable: true
        });
        ParameterGene.prototype.toStringContents = function () {
            return "{" + this._id + "}";
        };
        ParameterGene.prototype.clone = function () {
            return new ParameterGene(this._id, this._multiple);
        };
        ParameterGene.prototype.calculateWithoutMultiple = function (values) {
            return values[this._id];
        };
        ParameterGene.prototype.asReduced = function () {
            return this.clone();
        };
        ParameterGene.prototype.equals = function (other) {
            return this._id == other._id && this._multiple == other._multiple;
        };
        return ParameterGene;
    }(Gene_1.default));
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = ParameterGene;
});
//# sourceMappingURL=ParameterGene.js.map