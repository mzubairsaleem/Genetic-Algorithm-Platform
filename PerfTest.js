(function (dependencies, factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(dependencies, factory);
    }
})(["require", "exports", "typescript-dotnet-umd/System/Diagnostics/Stopwatch"], function (require, exports) {
    "use strict";
    var Stopwatch_1 = require("typescript-dotnet-umd/System/Diagnostics/Stopwatch");
    console.log("Elapsed Time:", Stopwatch_1.default.measure(function () {
        var n = 0;
        for (var j = 0; j < 10; j++) {
            for (var i = 0; i < 1000000000; i++) {
                n += i;
            }
        }
        console.log("Result: " + n);
    }).total.milliseconds);
});
//# sourceMappingURL=PerfTest.js.map