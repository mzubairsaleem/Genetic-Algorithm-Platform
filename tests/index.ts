///<reference path="import.d.ts"/>

import * as Operators from "../sample-problems/AlgebraBlackBox/Operators";
import OperatorGene from "../sample-problems/AlgebraBlackBox/Genes/Operator";
import ParameterGene from "../sample-problems/AlgebraBlackBox/Genes/ParameterGene";
import {convertParameterToAlphabet as formatGene} from "../sample-problems/AlgebraBlackBox/Environment";
{
	const assert = require('assert');

	describe("reduction", ()=>
	{

		it("should reduce simple summed parameters to minimum", ()=>
		{
			var o = new OperatorGene(Operators.ADD);
			o.add(new ParameterGene(0));
			o.add(new ParameterGene(0,-1));

			assert.equal(formatGene(o.toString()),"(a-a)");
			var r = o.asReduced();
			assert.equal(formatGene(r.toString()),"(0)")
		});

		it("should reduce simple summed parameters to minimum", ()=>
		{
			var o = new OperatorGene(Operators.ADD);
			o.add(new ParameterGene(0));
			o.add(new ParameterGene(0,-1));
			o.add(new ParameterGene(0,-1));

			assert.equal(formatGene(o.toString()),"(a-a-a)");
			var r = o.asReduced();
			assert.equal(formatGene(r.toString()),"-(a)")
		});

		it("should keep sign appropriately", ()=>
		{
			var o = new OperatorGene(Operators.ADD);
			var p = new OperatorGene(Operators.MULTIPLY);
			p.add(new ParameterGene(0));
			p.add(new ParameterGene(0));
			o.add(p);
			o.add(new ParameterGene(1,-1));

			assert.equal(formatGene(o.toString()),"((a*a)-b)");
			var r = o.asReduced();
			assert.equal(formatGene(r.toString()),"((a*a)-b)")
		});

	});
}