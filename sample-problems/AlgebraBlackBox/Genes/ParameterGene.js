"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Integer_1 = require("typescript-dotnet-umd/System/Integer");
var UnreducibleGene_1 = require("./UnreducibleGene");
var ParameterGene = (function (_super) {
    __extends(ParameterGene, _super);
    function ParameterGene(_id, multiple) {
        if (multiple === void 0) { multiple = 1; }
        var _this = _super.call(this, multiple) || this;
        _this._id = _id;
        Integer_1.default.assert(_id, 'id');
        return _this;
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
    ParameterGene.prototype.equals = function (other) {
        return other == this || other instanceof ParameterGene && this._id == other._id && this._multiple == other._multiple || _super.prototype.equals.call(this, other);
    };
    return ParameterGene;
}(UnreducibleGene_1.default));
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = ParameterGene;
//# sourceMappingURL=ParameterGene.js.map