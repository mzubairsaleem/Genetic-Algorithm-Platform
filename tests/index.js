"use strict";
var assert = require("assert");
var Operators = require("../sample-problems/AlgebraBlackBox/Operators");
var Operator_1 = require("../sample-problems/AlgebraBlackBox/Genes/Operator");
{
    describe("reduction", function () {
        it("should reduce simple summed parameters to minimum", function () {
            var o = new Operator_1.default("+", ["{0}", "-{0}"]);
            assert.equal(o.toAlphaParameters(), "(a-a)");
            var r = o.asReduced();
            assert.equal(r.toAlphaParameters(), "(0)");
        });
        it("should reduce to 1", function () {
            var o = new Operator_1.default(Operators.DIVIDE, ["{0}", "{0}"]);
            assert.equal(o.toAlphaParameters(), "(a/a)");
            var r = o.asReduced();
            assert.equal(r.toAlphaParameters(), "(1)");
        });
        it("should reduce simple summed parameters to minimum", function () {
            var o = new Operator_1.default("+", ["{0}", "-{0}", "-{0}"]);
            assert.equal(o.toAlphaParameters(), "(a-a-a)");
            var r = o.asReduced();
            assert.equal(r.toAlphaParameters(), "-(a)");
        });
        it("should keep sign appropriately", function () {
            var o = new Operator_1.default("+", [
                new Operator_1.default("*", ["{0}", "{0}"]),
                "-{1}"
            ]);
            assert.equal(o.toAlphaParameters(), "((a*a)-b)");
            var r = o.asReduced();
            assert.equal(r.toAlphaParameters(), "((a*a)-b)");
        });
        it("should reduce constants", function () {
            var o = new Operator_1.default("+", [
                new Operator_1.default("*", ["{0}", "{0}"]),
                "-{1}", 2, 3
            ]);
            assert.equal(o.toAlphaParameters(), "((a*a)-b+3+2)");
            var r = o.asReduced();
            assert.equal(r.toAlphaParameters(), "((a*a)-b+5)");
        });
        it("should reduce eliminate zero", function () {
            var o = new Operator_1.default("+", [
                new Operator_1.default("*", ["{0}", "{0}"]),
                "-{1}", 3, 2, -1, -4
            ]);
            assert.equal(o.toAlphaParameters(), "((a*a)-b+3+2-1-4)");
            var r = o.asReduced();
            assert.equal(r.toAlphaParameters(), "((a*a)-b)");
        });
        it("should pull out squares", function () {
            var o = new Operator_1.default("+", [
                new Operator_1.default(Operators.SQUARE_ROOT, [new Operator_1.default("*", ["{0}", "{0}"])]),
                new Operator_1.default(Operators.SQUARE_ROOT, [4]),
                new Operator_1.default(Operators.SQUARE_ROOT, [3]),
                new Operator_1.default(Operators.SQUARE_ROOT, [1]),
                new Operator_1.default(Operators.SQUARE_ROOT, [0])
            ]);
            assert.equal(o.toAlphaParameters(), "(√(a*a)+√0+√1+√3+√4)");
            var r = o.asReduced();
            assert.equal(r.toAlphaParameters(), "(a+√3+3)");
        });
        it("should pull out squared params", function () {
            var o = new Operator_1.default("+", [
                new Operator_1.default(Operators.SQUARE_ROOT, [
                    new Operator_1.default("*", ["{0}", "{0}", "{0}", "{0}", "{0}", "{1}", "{1}", "{1}"])
                ]),
                1
            ]);
            assert.equal(o.toAlphaParameters(), "(√(a*a*a*a*a*b*b*b)+1)");
            var r = o.asReduced();
            assert.equal(r.toAlphaParameters(), "((a*a*b*√(a*b))+1)");
        });
    });
}
//# sourceMappingURL=index.js.map