(function (dependencies, factory) {
    if (typeof module === 'object' && typeof module.exports === 'object') {
        var v = factory(require, exports); if (v !== undefined) module.exports = v;
    }
    else if (typeof define === 'function' && define.amd) {
        define(dependencies, factory);
    }
})(["require", "exports", "./Environment"], function (require, exports) {
    "use strict";
    var Environment_1 = require("./Environment");
    var env = new Environment_1.default();
    console.log("starting...");
    env.start();
    if (typeof document != "undefined") {
        var output_1 = document.getElementById("output");
        setInterval(function () {
            if (typeof env.state == "string")
                output_1.innerText = env.state;
        }, 200);
    }
});
//# sourceMappingURL=index.js.map