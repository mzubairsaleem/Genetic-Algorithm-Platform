"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Integer_1 = require("typescript-dotnet-umd/System/Integer");
var UnreducibleGene_1 = require("./UnreducibleGene");
var Types_1 = require("typescript-dotnet-umd/System/Types");
var RegularExpressions_1 = require("typescript-dotnet-umd/System/Text/RegularExpressions");
var PATTERN = new RegularExpressions_1.Regex("(?<multiple>-?\\d*){(?<id>\\d+)}");
var ParameterGene = (function (_super) {
    __extends(ParameterGene, _super);
    function ParameterGene(id, multiple) {
        if (multiple === void 0) { multiple = 1; }
        var _this;
        if (Types_1.Type.isString(id)) {
            var m = PATTERN.match(id);
            if (!m)
                throw "Unrecognized parameter pattern.";
            var groups = m.namedGroups;
            var pm = groups["multiple"].value;
            if (pm) {
                if (pm === "" || pm === "-")
                    pm += "1";
                multiple *= Number(pm);
            }
            id = Number(groups["id"].value);
        }
        _this = _super.call(this, multiple) || this;
        Integer_1.default.assert(id, 'id');
        _this._id = id;
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