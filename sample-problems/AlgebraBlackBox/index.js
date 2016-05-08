(function (factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(["require", "exports", "./Environment"], factory);
    }
})(function (require, exports) {
    "use strict";
    var Environment_1 = require("./Environment");
    var env = new Environment_1.default();
    console.log("starting...");
    env.execute();
});
//# sourceMappingURL=index.js.map