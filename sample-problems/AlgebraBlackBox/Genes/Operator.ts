/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */
import {List} from "typescript-dotnet-umd/System/Collections/List";
import Enumerable from "typescript-dotnet-umd/System.Linq/Linq";
import * as Procedure from "typescript-dotnet-umd/System/Collections/Array/Procedure";
import Type from "typescript-dotnet-umd/System/Types";
import AlgebraGene from "../Gene";
import ConstantGene from "./ConstantGene";
import ParameterGene from "./ParameterGene";
import * as Operator from "../Operators";
import {IEnumerableOrArray} from "typescript-dotnet-umd/System/Collections/IEnumerableOrArray";
import {Predicate} from "typescript-dotnet-umd/System/FunctionTypes";
import {CompareResult} from "typescript-dotnet-umd/System/CompareResult";
import {ILinqEnumerable} from "typescript-dotnet-umd/System.Linq/Enumerable";
import {startsWith, trim} from "typescript-dotnet-umd/System/Text/Utility";
import {Random} from "typescript-dotnet-umd/System/Random";

function arrange(a:AlgebraGene, b:AlgebraGene):CompareResult
{
	if(a instanceof ConstantGene && !(b instanceof ConstantGene))
		return 1;

	if(b instanceof ConstantGene && !(a instanceof ConstantGene))
		return -1;

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

function getRandomFrom<T>(source:ReadonlyArray<T>, excluding?:T):T
{
	return Random.select.one(excluding=== void(0) ? source : source.filter(
		v => v!==excluding), true);
}

function getRandomFromExcluding<T>(source:ReadonlyArray<T>, excluding:IEnumerableOrArray<T>):T
{
	let options:ReadonlyArray<T>;
	if(excluding)
	{
		const ex = Enumerable(excluding).memoize();
		options = source.filter(v => !ex.contains(v));
	}
	else
	{
		options = source;
	}

	return getRandomFrom(options);
}

function getRandomOperator(
	source:ReadonlyArray<OperatorSymbol>,
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
		if(this._operator!=value)
		{
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
		const s = this._source, children = s.slice();
		if(s.length>1 && this._operator!=Operator.DIVIDE)
		{
			children.sort(arrange);
		}
		return children;
	}

	reduce(reduceGroupings:boolean = false):boolean|AlgebraGene
	{
		let somethingDone = false;
		while(this._reduceLoop(reduceGroupings))
		{ somethingDone = true; }
		if(somethingDone) this._onModified();
		return somethingDone;
	}

	protected _reduceLoop(reduceGroupings:boolean = false):boolean
	{
		const _ = this, values:ILinqEnumerable<AlgebraGene> = _.linq;
		let somethingDone = false;

		// Look for groupings...
		Enumerable
			.from<AlgebraGene>(_._source.slice()) // use a copy...
			.groupBy(g => g.toStringContents())
			.where(g => g.count()>1)
			.forEach(p =>
			{
				let pa = p.memoize();

				switch(_._operator)
				{
					case Operator.ADD:
					{
						let sum = pa.sum(s => s.multiple);
						let hero = pa.first();

						if(sum==0)
							_._replaceInternal(hero, new ConstantGene(0));
						else
							hero.multiple = sum;

						pa.skip(1).forEach(g => _._removeInternal(g));

						somethingDone = true;
						break;
					}
					case Operator.MULTIPLY:
					{
						let g = p
							.ofType(OperatorGene)
							.where(g => g.operator==Operator.SQUARE_ROOT)
							.toArray();

						while(g.length>1)
						{
							_.remove(g.pop()!);
							let e = g.pop()!;
							_.replace(e, e.linq.single());
						}

						break;
					}

					case Operator.DIVIDE:
					{
						let first = pa.first();
						let next = pa.elementAt(1);
						if(!(first instanceof ConstantGene))
						{
							_._replaceInternal(first, new ConstantGene(first.multiple));
							_._replaceInternal(next, new ConstantGene(next.multiple));
							somethingDone = true;
						}
						break;
					}


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
					constGenes.forEach(g =>
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

				values.where(v => v.multiple!=1)
					.forEach(p =>
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
							g.linq
								.skip(1)
								.ofType(ParameterGene)
								.any(g2 =>
									mParams.any(g3 => g2.id==g3.id))
						)
						.firstOrDefault();

					if(divOperator!=null)
					{
						let oneToKill = divOperator.linq
							.skip(1)
							.ofType(ParameterGene)
							.where(p => mParams.any(pn => pn.id==p.id))
							.first();

						let mPToKill = mParams
							.where(p => p.id==oneToKill.id).first();

						divOperator.replace(oneToKill, new ConstantGene(oneToKill.multiple));
						_._replaceInternal(mPToKill, new ConstantGene(mPToKill.multiple));

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
				if(f && f.multiple==0)
				{
					_._multiple = 0;
					somethingDone = true;
					break;
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
				values.where(v => v.multiple<0).forEach(p =>
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
					let mSource:ILinqEnumerable<AlgebraGene>;

					switch(f.operator)
					{
						case Operator.MULTIPLY:
							mSource = f.linq;
							break;
						case Operator.DIVIDE:
							mSource = f.linq.take(1);
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
							let mPToKill = mParams.where(p => p.id==oneToKill!.id).first();
							_._replaceInternal(oneToKill, new ConstantGene(oneToKill.multiple));
							f.replace(mPToKill, new ConstantGene(mPToKill.multiple));

							somethingDone = true;
						}
					}

				}

				if(f instanceof ParameterGene)
				{
					let fP = <ParameterGene>f;
					let multiplyOperator = values
						.ofType(OperatorGene)
						.where(g => g.operator==Operator.MULTIPLY
						&& g.linq
							.ofType(ParameterGene)
							.any(g2 => g2.id==fP.id))
						.firstOrDefault();

					if(multiplyOperator!=null)
					{
						let oneToKill = multiplyOperator.linq
							.ofType(ParameterGene)
							.where(p => p.id==fP.id)
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
							g.linq.take(1)
								.ofType(ParameterGene)
								.any(g2 => g2.id==fP.id))
							.firstOrDefault();

						if(divOperator!=null)
						{
							let oneToKill = divOperator.linq.first();
							_._replaceInternal(f, new ConstantGene(f.multiple));
							divOperator.replace(oneToKill, new ConstantGene(oneToKill.multiple));

							somethingDone = true;
						}
					}


				}


			}

				// Clear out divisors that divide cleanly with the the current multiple.
			{
				let divisors = values.skip(1), g:AlgebraGene|undefined;

				// Start with any constant gene children.
				while(g = divisors.ofType(ConstantGene)
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

				// Still in case statement...
				for(let g of values
					.skip(1).ofType(OperatorGene)
					.where(p => p.operator==Operator.DIVIDE && p.count>1)
					.take(1).toArray())
				{

					let f = values.first();
					let fo:OperatorGene|null = f instanceof OperatorGene ? f : null;
					if(!fo || fo.operator!=Operator.MULTIPLY)
					{
						fo = new OperatorGene(Operator.MULTIPLY);

						_._replaceInternal(f, fo);
						fo.add(f);
						somethingDone = true;
					}

					// Here we have a classical inversion case that can help in reduction.
					for(let n of g.linq.skip(1).toArray())
					{

						g.remove(n);
						fo.add(n);

						somethingDone = true;
					}
				}
				break;

		}

		/* */

		const multiple = _._multiple;
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


			if(o.reduce(reduceGroupings))
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
				const opg = o.linq.firstOrDefault();

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
							&& og.linq.select(p => p.asReduced().toString())
								.distinct()
								.count()==1)
						{
							let firstChild = og.linq.first();
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


		// Pull out multiple to allow for reduction.
		if(_.count==1)
		{
			const p = values.first();
			if(_._operator==Operator.SQUARE_ROOT)
			{

				// if(p instanceof OperatorGene) {
				// 	let childCount = p.count;
				//
				// 	// Square root of square?
				// 	if(p.operator==Operator.MULTIPLY && childCount==2
				// 		&& p.linq.select(pp=>pp.asReduced().toString())
				// 			.distinct()
				// 			.count()==1)
				// 	{
				// 		return p.linq.first();
				//
				// 		// _._r
				// 		// p.remove(firstChild);
				// 		// p.operator = Operator.MULTIPLY;
				// 		// p.clear();
				// 		// p.add(firstChild);
				// 		//
				// 		// somethingDone = true;
				// 	}
				// }

				if(p instanceof ConstantGene)
				{
					if(p.multiple==0)
					{
						somethingDone = true;
						_.multiple *= p.multiple;
						p.multiple = 1;
					}
				}
			}
			else
			{
				if(!(p instanceof ConstantGene) && p.multiple!==1)
				{
					somethingDone = true;
					_.multiple *= p.multiple;
					p.multiple = 1;
				}
			}

		}

		if(_._operator==Operator.ADD && _._source.length>1)
		{
			let constants = Enumerable
				.from<AlgebraGene>(_._source.slice()) // use a copy...
				.ofType(ConstantGene);
			let len = constants.count();
			if(len)
			{

				let sum = constants.sum(s => s.multiple);
				let hero = constants.first();

				if(len>1)
				{
					hero.multiple = sum;
					constants.skip(1).forEach(c => _._removeInternal(c));
					len = 1;
					somethingDone = true;
				}

				// remove 0 if makes sense.
				if(sum==0 && _._source.length>len)
				{
					_._removeInternal(hero);
					somethingDone = true;
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

	private _reduced:OperatorGene|undefined;

	asReduced():OperatorGene
	{
		let r = this._reduced;
		if(!r)
		{
			const gene = this.clone();
			gene.reduce(true);
			this._reduced = r = gene.toString()===this.toString() ? this : gene;
		}
		return r;
	}


	resetToString():void
	{
		this._reduced = void 0;
		super.resetToString();
	}

	toStringContents():string
	{
		const _ = this;
		if(Operator.Available.Functions.indexOf(_._operator)!= -1)
		{
			if(_._source.length==1)
				return _.operator
					+ _._source[0].toString();
			else
				return _.operator
					+ parenGroup(_.arranged.map(s => s.toString()).join(","));
		}
		if(_._operator==Operator.ADD)
		{
			// Cleanup "+-"...
			return parenGroup(trim(_.arranged.map(s =>
			{
				let r = s.toString();
				if(!startsWith(r, '-'))
					r = "+" + r;
				return r;
			}).join(""), "+"));
		}
		else
			return parenGroup(_.arranged.map(s => s.toString()).join(_.operator));
	}


	clone():OperatorGene
	{
		const clone = new OperatorGene(this._operator, this._multiple);
		this.forEach(g =>
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

	protected calculateWithoutMultiple(values:ArrayLike<number>):number
	{
		const results = this._source.map(s => s.calculate(values));
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