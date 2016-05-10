///<reference path="../../../node_modules/typescript-dotnet/source/System/FunctionTypes"/>
///<reference path="../OperatorSymbol"/>

import List from "../../../node_modules/typescript-dotnet/source/System/Collections/List";
import Enumerable from "../../../node_modules/typescript-dotnet/source/System.Linq/Linq";
import * as Procedure from "../../../node_modules/typescript-dotnet/source/System/Collections/Array/Procedure";
import Integer from "../../../node_modules/typescript-dotnet/source/System/Integer";
import Type from "../../../node_modules/typescript-dotnet/source/System/Types";
import AlgebraGene from "../Gene";
import ConstantGene from "./ConstantGene";
import ParameterGene from "./ParameterGene";
import * as Operator from "../Operators";

function arrange(a:AlgebraGene, b:AlgebraGene):CompareResult
{
	if(a.multiple<b.multiple)
		return 1;

	if(a.multiple>b.multiple)
		return -1;

	let ats = a.toString();
	let bts = b.toString();
	if(ats==bts)
		return 0;

	let ts = [ats, bts];
	ts.sort();

	return ts[0]==ats ? -1 : +1;

}

function parenGroup(contents:string):string
{
	return "(" + contents + ")";
}

function getRandomFrom<T>(source:T[], excluding?:T):T
{
	return Integer.random.select(excluding=== void(0) ? source : source.filter(v=>v!==excluding));
}

function getRandomFromExcluding<T>(source:T[], excluding:IEnumerableOrArray<T>):T
{
	var options:T[];
	if(excluding)
	{
		var ex = Enumerable.from(excluding).memoize();
		options = source.filter(v=>!ex.contains(v));
	}
	else
	{
		options = source;
	}

	return getRandomFrom(options);
}

function getRandomOperator(
	source:OperatorSymbol[],
	excluded?:OperatorSymbol|IEnumerableOrArray<OperatorSymbol>):OperatorSymbol
{
	if(!excluded)
		return getRandomFrom(source);
	if(Type.isString(excluded))
		return getRandomFrom(source, excluded);
	else
		return getRandomFromExcluding(source, excluded);
}


class OperatorGene extends AlgebraGene
{
	constructor(
		private _operator:OperatorSymbol,
		multiple:number = 1)
	{
		super(multiple)
	}

	get operator():OperatorSymbol
	{
		return this._operator;
	}

	set operator(value:OperatorSymbol)
	{
		if(this._operator != value) {
			this._operator = value;
			this._onModified();
		}

	}

	isReducible():boolean
	{
		return true;
	}

	modifyChildren(closure:Predicate<List<AlgebraGene>>)
	{
		if(closure(this))
			this._onModified();
	}


	get arranged():AlgebraGene[]
	{
		var s = this._source, children = s.slice();
		if(s.length>1 && this._operator!=Operator.DIVIDE)
		{
			children.sort(arrange);
		}
		return children;
	}

	reduce(reduceGroupings:boolean = false):boolean
	{
		var somethingDone = false;
		while(this._reduceLoop(reduceGroupings))
		{ somethingDone = true; }
		if(somethingDone) this._onModified();
		return somethingDone;
	}

	protected _reduceLoop(reduceGroupings:boolean = false):boolean
	{
		var _ = this, values = _.asEnumerable(), somethingDone = false;

		Enumerable
			.from(_._source.slice()) // use a copy...
			.groupBy(g=>g.toStringContents())
			.where(g=>g.count()>1)
			.forEach(p=>
			{
				let pa = p.memoize();

				switch(_._operator)
				{
					case Operator.ADD:
					{
						let sum = pa.sum(s => s.multiple);
						let hero = pa.first();

						if(sum==0)
							_.replace(hero, new ConstantGene(0));
						else
							hero.multiple = sum;

						pa.skip(1).forEach(g=>_._removeInternal(g));

						somethingDone = true;
					}
						break;

					case Operator.DIVIDE:
					{
						let first = _._source[0];
						let hero = pa.first();
						if(!(first instanceof ConstantGene)
							&& first.toStringContents()==hero.toStringContents())
						{
							_.replace(first, new ConstantGene(first.multiple));
							_.replace(hero, new ConstantGene(hero.multiple));
							somethingDone = true;
						}
					}
						break;

				}
			});


		switch(_._operator)
		{
			case Operator.MULTIPLY:
			{
				if(values.any(v => !v.multiple))
				{
					_._multiple = 0;
					somethingDone = true;
					break;
				}

				let constGenes = values
					.ofType(ConstantGene)
					.memoize();

				if(_.count>1 && constGenes.any())
				{
					constGenes.forEach(g=>
					{
						_._removeInternal(g);
						_._multiple *= g.multiple;
						somethingDone = true;
					});

					if(!_.count)
					{
						_._addInternal(new ConstantGene(_._multiple));
						_._multiple = 1;
						somethingDone = true;
					}
				}

				values.where(v=>v.multiple!=1)
					.forEach(p=>
					{
						somethingDone = true;
						_.multiple *= p.multiple;
						p.multiple = 1;
					});


				let mParams = values
					.ofType(ParameterGene)
					.memoize();

				if(mParams.any())
				{

					let divOperator = values
						.ofType(OperatorGene)
						.where(g =>
							g.operator==Operator.DIVIDE &&
							g.asEnumerable()
								.skip(1)
								.ofType(ParameterGene)
								.any(g2 =>
									mParams.any(g3 => g2.id==g3.id))
						)
						.firstOrDefault();

					if(divOperator!=null)
					{
						let oneToKill = divOperator.asEnumerable()
							.skip(1)
							.ofType(ParameterGene)
							.where(p => mParams.any(pn => pn.id==p.id))
							.first();

						let mPToKill = mParams
							.where(p => p.id==oneToKill.id).first();

						divOperator.replace(oneToKill, new ConstantGene(oneToKill.multiple));
						_.replace(mPToKill, new ConstantGene(mPToKill.multiple));

						somethingDone = true;
					}
				}


			}
				break;


			case Operator.DIVIDE:
			{
				let f = values.firstOrDefault();

				// Migrate the multiple outward.
				if(f!=null && f.multiple!=1)
				{
					_._multiple *= f.multiple;
					f.multiple = 1;
					somethingDone = true;
					break;
				}

				// If the first entry has a zero multiple.
				if(values.take(1).any(v => v.multiple==0))
				{
					_._multiple = 0;
					somethingDone = true;
				}

				// Check for divide by zero.
				if(values.skip(1).any(v => v.multiple==0))
				{
					_._multiple = NaN;  // It is possible to specify positive or negative infinity...
					somethingDone = true;
				}

				// Break out if necessary..
				{
					let multiple = _._multiple;
					if(multiple==0 || isNaN(multiple))
						break; // break out of case..  No need to proceed.
				}

				// look for sign inversion and simplify.
				values.where(v => v.multiple<0).forEach(p=>
				{
					somethingDone = true;
					_._multiple *= -1;
					p.multiple *= -1;
				});

				// Pull out all multiples and group them into one constant.
				{
					let newConst = values
						.ofType(ConstantGene)
						.lastOrDefault();

					let havMultiples = values
						.skip(1)
						.where(v => v.multiple!=1 && v!=newConst).toArray();

					if(havMultiples.length)
					{

						if(!newConst)
						{
							newConst = new ConstantGene(1);
							_._addInternal(newConst);
						}

						for(let g of havMultiples)
						{
							newConst.multiple *= g.multiple;
							g.multiple = 1;
							if(g instanceof ConstantGene)
								_._removeInternal(g);
						}

						somethingDone = true;
					}
				}

				if(f instanceof OperatorGene)
				{
					let mSource:Enumerable<AlgebraGene> = null;

					switch(f.operator)
					{
						case Operator.MULTIPLY:
							mSource = f.asEnumerable();
							break;
						case Operator.DIVIDE:
							mSource = f.asEnumerable().take(1);
							break;
						default:
							mSource = Enumerable.empty<AlgebraGene>();
							break;
					}

					let mParams = mSource
						.ofType(ParameterGene)
						.memoize();

					if(mParams.any())
					{
						let oneToKill = values
							.ofType(ParameterGene)
							.where(p => mParams.any(pn => pn.id==p.id))
							.firstOrDefault();

						if(oneToKill!=null)
						{
							let mPToKill = mParams.where(p => p.id==oneToKill.id).first();
							_._replaceInternal(oneToKill, new ConstantGene(oneToKill.multiple));
							f.replace(mPToKill, new ConstantGene(mPToKill.multiple));

							somethingDone = true;
						}
					}

				}

				if(f instanceof ParameterGene)
				{
					let multiplyOperator = values
						.ofType(OperatorGene)
						.where(g => g.operator==Operator.MULTIPLY
						&& g.asEnumerable()
							.ofType(ParameterGene)
							.any(g2 => g2.id==f.id))
						.firstOrDefault();

					if(multiplyOperator!=null)
					{
						let oneToKill = multiplyOperator.asEnumerable()
							.ofType(ParameterGene)
							.where(p => p.id==f.id)
							.first();

						_._replaceInternal(f, new ConstantGene(f.multiple));
						multiplyOperator.replace(oneToKill, new ConstantGene(oneToKill.multiple));

						somethingDone = true;
					}
					else
					{
						let divOperator = values
							.ofType(OperatorGene)
							.where(g =>
							g.operator==Operator.DIVIDE &&
							g.asEnumerable().take(1)
								.ofType(ParameterGene)
								.any(g2 => g2.id==f.id))
							.firstOrDefault();

						if(divOperator!=null)
						{
							let oneToKill = divOperator.asEnumerable().first();
							_._replaceInternal(f, new ConstantGene(f.multiple));
							divOperator.replace(oneToKill, new ConstantGene(oneToKill.multiple));

							somethingDone = true;
						}
					}


				}


			}

				// Clear out divisors that divide cleanly with the the current multiple.
			{
				let divisors = values.skip(1), g:AlgebraGene;

				// Start with any constant gene children.
				while(g
					= divisors.ofType(ConstantGene)
					.where(p => _._multiple%p.multiple==0)
					.firstOrDefault())
				{
					_._multiple /= g.multiple;
					_._removeInternal(g);
					somethingDone = true;
				}

				// Then adjust for other multiples.
				while(g
					= divisors
					.where(p => p.multiple!=1 && _._multiple%p.multiple==0)
					.firstOrDefault())
				{
					_._multiple /= g.multiple;
					g.multiple = 1;
					somethingDone = true;
				}

			}


				for(let g of values
					.skip(1).ofType(OperatorGene)
					.where(p => p.operator==Operator.DIVIDE && p.count>1)
					.take(1).toArray())
				{

					let f = values.first();
					let fo:OperatorGene = f instanceof OperatorGene ? f : null;
					if(!fo || fo.operator!=Operator.MULTIPLY)
					{
						fo = new OperatorGene(Operator.MULTIPLY);

						_._replaceInternal(f, fo);
						fo.add(f);
						somethingDone = true;
					}

					// Here we have a classical inversion case that can help in reduction.
					for(let n of g.asEnumerable().skip(1).toArray())
					{

						g.remove(n);
						fo.add(n);

						somethingDone = true;
					}
				}
				break;

		}

		/* */

		var multiple = _._multiple;
		if(multiple==0 || !isFinite(multiple) || isNaN(multiple))
		{
			if(values.any())
			{
				_._clearInternal();
				somethingDone = true;
			}
		}


		for(let o of values
			.ofType(OperatorGene)
			.toArray())
		{
			if(!values.contains(o))
				continue;


			if(o.reduce())
				somethingDone = true;

			if(o.count==0)
			{
				somethingDone = true;

				_._replaceInternal(o, new ConstantGene(o.multiple));
				continue;
			}


			if(reduceGroupings)
			{
				if(_._operator==Operator.ADD)
				{
					if(o.multiple==0)
					{
						_._removeInternal(o);
						somethingDone = true;
						continue;
					}

					if(o.operator==Operator.ADD)
					{
						let index = _._source.indexOf(o);
						for(let og of o.toArray())
						{
							o.remove(og);
							og.multiple *= o.multiple;
							_.insert(index, og);
						}
						_._removeInternal(o);

						somethingDone = true;
						continue;
					}
				}

				if(_._operator==Operator.MULTIPLY)
				{

					if(o.operator==Operator.MULTIPLY)
					{
						_._multiple *= o._multiple;

						let index = _._source.indexOf(o);
						for(let og of o.toArray())
						{
							o.remove(og);
							_.insert(index, og);
						}
						_._removeInternal(o);

						somethingDone = true;
						continue;
					}
				}
			}

			if(o.count==1)
			{

				var opg = o.asEnumerable().firstOrDefault();

				if(opg instanceof ParameterGene || opg instanceof ConstantGene)
				{
					if(Operator.Available.Functions.indexOf(o.operator)== -1)
					{

						o.remove(opg);
						opg.multiple *= o.multiple;
						this._replaceInternal(o, opg);

						somethingDone = true;
						continue;

					}
					else if(o.operator==Operator.SQUARE_ROOT && opg instanceof ConstantGene && opg.multiple>=0)
					{
						let sq = Math.sqrt(opg.multiple);
						for(let i = 0; i<sq; i++)
						{
							if(i==sq)
							{
								o.remove(opg);
								this._replaceInternal(o, opg);
								somethingDone = true;
							}

						}
					}
				}

				if(opg instanceof OperatorGene)
				{
					let og = <OperatorGene>opg;

					if(o.operator==Operator.SQUARE_ROOT)
					{

						let childCount = og.count;

						// Square root of square?
						if(og.operator==Operator.MULTIPLY && childCount==2
							&& og.asEnumerable().select(p=>p.asReduced().toString())
								.distinct()
								.count()==1)
						{
							let firstChild = og.asEnumerable().ofType(ParameterGene).first();
							og.remove(firstChild);
							o.operator = Operator.MULTIPLY;
							o.clear();
							o.add(firstChild);

							somethingDone = true;
						}

					}
					else if(Operator.Available.Operators.indexOf(o.operator)!= -1)
					{
						let children = og.toArray();
						o.operator = og.operator;
						o.multiple *= og.multiple;
						og.clear();
						o.clear();
						o.importEntries(children);

						somethingDone = true;
					}
				}


			}
		}


		if(_._operator!=Operator.DIVIDE)
		{
			_._source.sort(arrange);
		}

		if(somethingDone)
			_._onModified();

		return somethingDone;
	}

	private _reduced:OperatorGene;
	asReduced():OperatorGene
	{
		var r = this._reduced;
		if(!r) {
			var gene = this.clone();
			gene.reduce();
			this._reduced = r = gene.toString()===this.toString() ? this : gene;
		}
		return r;
	}


	resetToString():void
	{
		this._reduced = null;
		super.resetToString();
	}

	toStringContents():string
	{
		var _ = this;
		if(Operator.Available.Functions.indexOf(_._operator)!= -1)
		{
			if(_._source.length==1)
				return _.operator
					+ _._source[0].toString();
			else
				return _.operator
					+ parenGroup(_.arranged.map(s=>s.toString()).join(","));
		}
		return parenGroup(_.arranged.map(s=>s.toString()).join(_.operator));
	}


	clone():OperatorGene
	{
		var clone = new OperatorGene(this._operator, this._multiple);
		this.forEach(g=>
		{
			clone._addInternal(g.clone());
		});
		return clone;
	}

	static getRandomOperator(excluded?:OperatorSymbol|IEnumerableOrArray<OperatorSymbol>):OperatorSymbol
	{
		return getRandomOperator(Operator.Available.Operators, excluded);
	}

	static getRandomFunctionOperator(excluded?:OperatorSymbol|IEnumerableOrArray<OperatorSymbol>):OperatorSymbol
	{
		return getRandomOperator(Operator.Available.Functions, excluded);
	}

	static getRandomOperation(excluded?:OperatorSymbol|IEnumerableOrArray<OperatorSymbol>):OperatorGene
	{
		return new OperatorGene(OperatorGene.getRandomOperator(excluded));
	}

	protected calculateWithoutMultiple(values:number[]):number
	{
		var results = this._source.map(s => s.calculate(values));
		switch(this._operator)
		{

			case Operator.ADD:
				return Procedure.sum(results);

			// For now "Power Of" functions will be elaborated with a*a.
			case Operator.MULTIPLY:
				return Procedure.product(results);

			case Operator.DIVIDE:
				return Procedure.quotient(results);

			// Single value functions...
			case Operator.SQUARE_ROOT:
				if(results.length==1)
					return Math.sqrt(results[0]);
				break;
		}

		return NaN;
	}


}

export default OperatorGene;