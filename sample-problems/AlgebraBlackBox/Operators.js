"use strict";
exports.ADD = "+", exports.MULTIPLY = "*", exports.DIVIDE = "/", exports.SQUARE_ROOT = "√";
var Available;
(function (Available) {
    Available.Operators = Object.freeze([
        exports.ADD,
        exports.MULTIPLY,
        exports.DIVIDE
    ]);
    Available.Functions = Object.freeze([
        exports.SQUARE_ROOT
    ]);
    Available.FunctionActual = Object.freeze([
        "Math.sqrt"
    ]);
})(Available = exports.Available || (exports.Available = {}));
//# sourceMappingURL=Operators.js.map