(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports"], factory);
    }
})(function (require, exports) {
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
    })(Available = exports.Available || (exports.Available = {}));
});
//# sourceMappingURL=Operators.js.map