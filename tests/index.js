"use strict";
var Operators = require("../sample-problems/AlgebraBlackBox/Operators");
var Operator_1 = require("../sample-problems/AlgebraBlackBox/Genes/Operator");
var ParameterGene_1 = require("../sample-problems/AlgebraBlackBox/Genes/ParameterGene");
var Environment_1 = require("../sample-problems/AlgebraBlackBox/Environment");
var ConstantGene_1 = require("../sample-problems/AlgebraBlackBox/Genes/ConstantGene");
{
    var assert_1 = require('assert');
    describe("reduction", function () {
        it("should reduce simple summed parameters to minimum", function () {
            var o = new Operator_1.default(Operators.ADD);
            o.add(new ParameterGene_1.default(0));
            o.add(new ParameterGene_1.default(0, -1));
            assert_1.equal(Environment_1.convertParameterToAlphabet(o.toString()), "(a-a)");
            var r = o.asReduced();
            assert_1.equal(Environment_1.convertParameterToAlphabet(r.toString()), "(0)");
        });
        it("should reduce to 1", function () {
            var o = new Operator_1.default(Operators.DIVIDE);
            o.add(new ParameterGene_1.default(0));
            o.add(new ParameterGene_1.default(0));
            assert_1.equal(Environment_1.convertParameterToAlphabet(o.toString()), "(a/a)");
            var r = o.asReduced();
            assert_1.equal(Environment_1.convertParameterToAlphabet(r.toString()), "(1)");
        });
        it("should reduce simple summed parameters to minimum", function () {
            var o = new Operator_1.default(Operators.ADD);
            o.add(new ParameterGene_1.default(0));
            o.add(new ParameterGene_1.default(0, -1));
            o.add(new ParameterGene_1.default(0, -1));
            assert_1.equal(Environment_1.convertParameterToAlphabet(o.toString()), "(a-a-a)");
            var r = o.asReduced();
            assert_1.equal(Environment_1.convertParameterToAlphabet(r.toString()), "-(a)");
        });
        it("should keep sign appropriately", function () {
            var o = new Operator_1.default(Operators.ADD);
            var p = new Operator_1.default(Operators.MULTIPLY);
            p.add(new ParameterGene_1.default(0));
            p.add(new ParameterGene_1.default(0));
            o.add(p);
            o.add(new ParameterGene_1.default(1, -1));
            assert_1.equal(Environment_1.convertParameterToAlphabet(o.toString()), "((a*a)-b)");
            var r = o.asReduced();
            assert_1.equal(Environment_1.convertParameterToAlphabet(r.toString()), "((a*a)-b)");
        });
        it("should reduce constants", function () {
            var o = new Operator_1.default(Operators.ADD);
            var p = new Operator_1.default(Operators.MULTIPLY);
            p.add(new ParameterGene_1.default(0));
            p.add(new ParameterGene_1.default(0));
            o.add(p);
            o.add(new ParameterGene_1.default(1, -1));
            o.add(new ConstantGene_1.default(2));
            o.add(new ConstantGene_1.default(3));
            assert_1.equal(Environment_1.convertParameterToAlphabet(o.toString()), "((a*a)-b+3+2)");
            var r = o.asReduced();
            assert_1.equal(Environment_1.convertParameterToAlphabet(r.toString()), "((a*a)-b+5)");
        });
        it("should reduce eliminate zero", function () {
            var o = new Operator_1.default(Operators.ADD);
            var p = new Operator_1.default(Operators.MULTIPLY);
            p.add(new ParameterGene_1.default(0));
            p.add(new ParameterGene_1.default(0));
            o.add(p);
            o.add(new ParameterGene_1.default(1, -1));
            o.add(new ConstantGene_1.default(2));
            o.add(new ConstantGene_1.default(3));
            o.add(new ConstantGene_1.default(-1));
            o.add(new ConstantGene_1.default(-4));
            assert_1.equal(Environment_1.convertParameterToAlphabet(o.toString()), "((a*a)-b+3+2-1-4)");
            var r = o.asReduced();
            assert_1.equal(Environment_1.convertParameterToAlphabet(r.toString()), "((a*a)-b)");
        });
    });
}
//# sourceMappingURL=index.js.map