"use strict";
var Operators = require("../sample-problems/AlgebraBlackBox/Operators");
var Operator_1 = require("../sample-problems/AlgebraBlackBox/Genes/Operator");
var ParameterGene_1 = require("../sample-problems/AlgebraBlackBox/Genes/ParameterGene");
var Environment_1 = require("../sample-problems/AlgebraBlackBox/Environment");
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
    });
}
//# sourceMappingURL=index.js.map