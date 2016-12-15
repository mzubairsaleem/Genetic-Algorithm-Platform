///<reference types="mocha"/>
///<reference types="assert"/>
///<reference types="node"/>
const assert = require("assert");

import * as Operators from "../sample-problems/AlgebraBlackBox/Operators";
import OperatorGene from "../sample-problems/AlgebraBlackBox/Genes/Operator";
import ParameterGene from "../sample-problems/AlgebraBlackBox/Genes/ParameterGene";
import ConstantGene from "../sample-problems/AlgebraBlackBox/Genes/ConstantGene";
{

	describe("reduction", () =>
	{

		it("should reduce simple summed parameters to minimum", () =>
		{
			const o = new OperatorGene("+", ["{0}", "-{0}"]);

			assert.equal(o.toAlphaParameters(), "(a-a)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(), "(0)")
		});

		it("should reduce to 1", () =>
		{
			const o = new OperatorGene(Operators.DIVIDE, ["{0}", "{0}"]);

			assert.equal(o.toAlphaParameters(), "(a/a)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(), "(1)")
		});

		it("should reduce simple summed parameters to minimum", () =>
		{
			const o = new OperatorGene("+", ["{0}", "-{0}", "-{0}"]);

			assert.equal(o.toAlphaParameters(), "(a-a-a)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(), "-(a)")
		});

		it("should keep sign appropriately", () =>
		{
			const o = new OperatorGene("+", [
				new OperatorGene("*", ["{0}", "{0}"]),
				"-{1}"
			]);

			assert.equal(o.toAlphaParameters(), "((a*a)-b)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(), "((a*a)-b)")
		});

		it("should reduce constants", () =>
		{
			const o = new OperatorGene("+", [
				new OperatorGene("*", ["{0}", "{0}"]),
				"-{1}", 2, 3
			]);

			assert.equal(o.toAlphaParameters(), "((a*a)-b+3+2)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(), "((a*a)-b+5)")
		});

		it("should reduce eliminate zero", () =>
		{
			const o = new OperatorGene("+", [
				new OperatorGene("*", ["{0}", "{0}"]),
				"-{1}", 3, 2, -1, -4
			]);

			assert.equal(o.toAlphaParameters(), "((a*a)-b+3+2-1-4)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(), "((a*a)-b)");
		});


		it("should pull out squares", () =>
		{
			const o = new OperatorGene("+", [
				new OperatorGene(Operators.SQUARE_ROOT, [new OperatorGene("*", ["{0}", "{0}"])]),
				new OperatorGene(Operators.SQUARE_ROOT, [4]),
				new OperatorGene(Operators.SQUARE_ROOT, [3]),
				new OperatorGene(Operators.SQUARE_ROOT, [1]),
				new OperatorGene(Operators.SQUARE_ROOT, [0])
			]);

			assert.equal(o.toAlphaParameters(), "(√(a*a)+√0+√1+√3+√4)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(), "(a+√3+3)");
		});

		it("should pull out squared params", () =>
		{
			const o = new OperatorGene("+", [
				new OperatorGene(Operators.SQUARE_ROOT, [
					new OperatorGene("*", ["{0}", "{0}", "{0}", "{0}", "{0}", "{1}", "{1}", "{1}"])
				]),
				1
			]);

			assert.equal(o.toAlphaParameters(), "(√(a*a*a*a*a*b*b*b)+1)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(), "((a*a*b*√(a*b))+1)");
		});


	});
}