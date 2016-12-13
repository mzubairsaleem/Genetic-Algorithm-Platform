///<reference types="mocha"/>
///<reference types="assert"/>
///<reference types="node"/>
const assert = require("assert");

import * as Operators from "../sample-problems/AlgebraBlackBox/Operators";
import OperatorGene from "../sample-problems/AlgebraBlackBox/Genes/Operator";
import ParameterGene from "../sample-problems/AlgebraBlackBox/Genes/ParameterGene";
import ConstantGene from "../sample-problems/AlgebraBlackBox/Genes/ConstantGene";
{

	describe("reduction", ()=>
	{

		it("should reduce simple summed parameters to minimum", ()=>
		{
			const o = new OperatorGene(Operators.ADD);
			o.add(new ParameterGene(0));
			o.add(new ParameterGene(0,-1));

			assert.equal(o.toAlphaParameters(),"(a-a)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(),"(0)")
		});

		it("should reduce to 1", ()=>
		{
			const o = new OperatorGene(Operators.DIVIDE);
			o.add(new ParameterGene(0));
			o.add(new ParameterGene(0));

			assert.equal(o.toAlphaParameters(),"(a/a)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(),"(1)")
		});

		it("should reduce simple summed parameters to minimum", ()=>
		{
			const o = new OperatorGene(Operators.ADD);
			o.add(new ParameterGene(0));
			o.add(new ParameterGene(0,-1));
			o.add(new ParameterGene(0,-1));

			assert.equal(o.toAlphaParameters(),"(a-a-a)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(),"-(a)")
		});

		it("should keep sign appropriately", ()=>
		{
			const o = new OperatorGene(Operators.ADD);
			const p = new OperatorGene(Operators.MULTIPLY);
			p.add(new ParameterGene(0));
			p.add(new ParameterGene(0));
			o.add(p);
			o.add(new ParameterGene(1,-1));

			assert.equal(o.toAlphaParameters(),"((a*a)-b)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(),"((a*a)-b)")
		});

		it("should reduce constants", ()=>
		{
			const o = new OperatorGene(Operators.ADD);
			const p = new OperatorGene(Operators.MULTIPLY);
			p.add(new ParameterGene(0));
			p.add(new ParameterGene(0));
			o.add(p);
			o.add(new ParameterGene(1,-1));
			o.add(new ConstantGene(2));
			o.add(new ConstantGene(3));

			assert.equal(o.toAlphaParameters(),"((a*a)-b+3+2)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(),"((a*a)-b+5)")
		});

		it("should reduce eliminate zero", ()=>
		{
			const o = new OperatorGene(Operators.ADD);
			const p = new OperatorGene(Operators.MULTIPLY);
			p.add(new ParameterGene(0));
			p.add(new ParameterGene(0));
			o.add(p);
			o.add(new ParameterGene(1,-1));
			o.add(new ConstantGene(2));
			o.add(new ConstantGene(3));
			o.add(new ConstantGene(-1));
			o.add(new ConstantGene(-4));

			assert.equal(o.toAlphaParameters(),"((a*a)-b+3+2-1-4)");
			const r = o.asReduced();
			assert.equal(r.toAlphaParameters(),"((a*a)-b)")
		});

	});
}