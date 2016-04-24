///<reference path="../../../node_modules/typescript-dotnet/source/System/FunctionTypes"/>

import * as Enumerable from "../../../node_modules/typescript-dotnet/source/System.Linq/Linq";
import * as Procedure from "../../../node_modules/typescript-dotnet/source/System/Collections/Array/Procedure";
import Integer from "../../../node_modules/typescript-dotnet/source/System/Integer";
import AlgebraGene from "../Gene";
import ConstantGene from "./ConstantGene";

const ADDITION:string = "+", // Subtraction is simply a negative multiplier.
      MULTIPLICATION:string = "*",
      DIVISION:string = "/",
      SQUARE_ROOT:string = "√";

type OperatorString = ADDITION | MULTIPLICATION | DIVISION | SQUARE_ROOT;

const AvailableOperators:OperatorString[] = Object.freeze([ADDITION, MULTIPLICATION, DIVISION]);
const AvailableFunctions:OperatorString[] = Object.freeze([ SQUARE_ROOT ]);

function arrange(a:AlgebraGene, b:AlgebraGene):CompareResult {
	return b.multiple > a.multiple || b.toString() > a.toString();
}

function parenGroup(contents:string):string {
	return "("+contents+")";
}

function getRandomFrom<T>(source:T[], excluding?:T):T
{
	return Integer.random.select(excluding===void(0) ? source : source.filter(v=>v!==excluding));
}


function getRandomFromExcluding<T>(source:T[], excluding:IEnumerableOrArray<T>):T
{
	var options:T[];
	if(excluding) {
		var ex = Enumerable.from(excluding).memoize();
		options = source.filter(v=>!ex.contains(v));
	} else {
		options = source;
	}

	return getRandomFrom(options);
}



class OperatorGene extends AlgebraGene
{

	constructor(
		private _operator:OperatorString,
		multiple:number = 1) {
		super(multiple)
	}

	get operator():OperatorString
	{
		return this._operator;
	}

	set operator(value:OperatorString)
	{
		this._operator = value;
		this._onModified();
	}

	modifyValues(closure:Action<List<Gene>>)
	{
		closure(values);
		this._onModified();
	}


	get childrenArranged():AlgebraGene[]
		{
			var s = this._source, children = s.slice();
			if(s.length>1 && this._operator!=DIVISION) {
				children.sort(arrange);
			}
			return children;
		}

	reduce(reduceGroupings:boolean = false):boolean
		{
			var somethingDone = false;
			while (this.reduceLoop(reduceGroupings)) { somethingDone = true; }
			if(somethingDone) this._onModified();
			return somethingDone;
		}

	protected _reduceLoop(reduceGroupings:boolean = false):boolean
		{
			var _ = this, values = this.asEnumerable(), somethingDone = false;

			Enumerable
				.from(this._source.slice()) // use a copy...
				.groupBy(g=>g.toStringContents())
				.where(g=>g.count()>1)
				.forEach(p=>{
					let pa = p.memoize();

					switch (this.operator)
					{
						case ADDITION:
							{
								let sum = pa.sum(s => s.multiple);
								let hero = pa.first();

								if (sum == 0)
									_.replace(hero, new ConstantGene(0));
								else
									hero.multiple = sum;

								pa.skip(1).forEach(g=>_._removeInternal(g));

								somethingDone = true;
							}
						break;

						case DIVISION:
							{
								let first = _._source[0];
								let hero = pa.first();
								if (!(first instanceof ConstantGene)
									&& first.toStringContents() == hero.toStringContents())
								{
									_.replace(first, new ConstantGene(first.multiple));
									_.replace(hero, new ConstantGene(hero.multiple));
									somethingDone = true;
								}
							}
							break;

					}
				});


			switch (this.operator)
			{
				case MULTIPLICATION:
				{
					if (values.any(v => !v.multiple))
					{
						_.multiple = 0;
						somethingDone = true;
						break;
					}

					let constGenes = values
						.ofType(ConstantGene)
						.memoize();

					if (_.count > 1 && constGenes.any())
					{
						constGenes.forEach(g=>{
							_._removeInternal(g);
							_._multiple *= g.multiple;
							somethingDone = true;
						});

						if (!_.count)
						{
							_._addInternal(new ConstantGene(_._multiple));
							_._multiple = 1;
							somethingDone = true;
						}
					}

					values.where(v=>v.multiple!=1)
						.forEach(p=>{
							somethingDone = true;
							_.multiple *= p.multiple;
							p.multiple = 1;
						});


					let mParams = values
						.ofType(ParameterGene)
						.memoize();

					if (mParams.any())
					{

						let divOperator = values
							.ofType(OperatorGene)
							.where(g =>
								g.operator == DIVISION &&
								g.asEnumerable()
									.skip(1)
									.ofType(ParameterGene)
									.any(g2 =>
										mParams.any(g3 =>
											g2.id == g3.id))
							)
							.firstOrDefault();

						if (divOperator != null)
						{
							let oneToKill = divOperator.Values
								.skip(1)
								.ofType(ParameterGene)
							.where(p => mParams.any(pn => pn.id == p.id))
							.first();

							let mPToKill = mParams.first(p => p.hash == oneToKill.hash);

							divOperator.replace(oneToKill, new ConstantGene(oneToKill.multiple));
							this.replace(mPToKill, new ConstantGene(mPToKill.multiple));

							somethingDone = true;
						}
					}


				}
					break;


				case DIVISION:
				{
					let f = values.firstOrDefault();
					if (f != null && f.multiple != 1)
					{
						this._multiple *= f.multiple;
						f.multiple = 1;
						somethingDone = true;
						break;
					}

					if(values.take(1).any(v => v.multiple == 0)){
						this._multiple = 0;
						somethingDone = true;
					}

					if(values.skip(1).any(v => v.multiple == 0)){
					{
						this._multiple = NaN;  // It is possible to specify positive or negative infinity...
						somethingDone = true;
					}
						let multiple = this._multiple;

					if (multiple == 0 || isNaN(multiple))
						break;

					values.where(v => v.multiple < 0).forEach(p=>
					{
						somethingDone = true;
						this._multiple *= -1;
						p.multiple *= -1;
					});

					let newConst = values.LastOrDefault() as ConstantGene;
					let havMultiples = values.skip(1).where(v => v._multiple != 1 && v!=newConst).toArray();
					if(havMultiples.any()){

						if (newConst == null) {
							newConst = new ConstantGene(1);
							values.Add(newConst);
						}

						for(let g of havMultiples)
						{
							newConst.multiple *= g.multiple;
							g.multiple = 1;
							if (g instanceof ConstantGene)
							this._removeInternal(g);
						}

						somethingDone = true;
					}


					let fo = f as OperatorGene;

					if (fo != null)
					{
						IEnumerable<Gene> msource = null;

						switch (fo.operator)
						{
							case MULTIPLICATION:
								msource = fo.values;
								break;
							case DIVISION:
								msource = fo.values.take(1);
								break;
							default:
								msource = Enumerable.Empty<Gene>();
								break;
						}

						let mParams = msource
							.ofType(ParameterGene)
						.toArray();

						if (mParams.any())
						{
							let oneToKill = values
								.ofType(ParameterGene)
							.where(p => mParams.any(pn => pn.id == p.id))
							.FirstOrDefault();

							if (oneToKill != null)
							{
								let mPToKill = mParams.First(p => p.id == oneToKill.id);
								Replace(oneToKill, new ConstantGene(oneToKill.multiple));
								fo.Replace(mPToKill, new ConstantGene(mPToKill.multiple));

								somethingDone = true;
							}
						}

					}

					let fp = f as ParameterGene;

					if (fp != null)
					{
						let multOperator = values
							.ofType(OperatorGene)
						.where(g => g.operator == MULTIPLICATION
						&& g.values
							.ofType(ParameterGene)
						.any(g2 => g2.id == fp.id))
					.FirstOrDefault();

						if (multOperator != null)
						{
							let oneToKill = multOperator.values
								.ofType(ParameterGene)
							.where(p => p.id == fp.id)
							.first();

							Replace(fp, new ConstantGene(fp.multiple));
							multOperator.Replace(oneToKill, new ConstantGene(oneToKill.multiple));

							somethingDone = true;
						}
						else
						{
							let divOperator = values
								.ofType(OperatorGene)
							.where(g => g.operator == DIVISION
							&& g.asEnumerable().take(1)
								.ofType(ParameterGene)
							.any(g2 => g2.id == fp.id))
						.firstOrDefault();

							if (divOperator != null)
							{
								let oneToKill = divOperator.asEnumerable().first();


								Replace(fp, new ConstantGene(fp.multiple));
								divOperator.Replace(oneToKill, new ConstantGene(oneToKill.multiple));

								somethingDone = true;
							}
						}


					}

					for(let g of values.skip(1).ofType(ConstantGene).where(p => this._multiple % p.multiple == 0).toArray())
					{
						this._multiple /= g.multiple;
						this._removeInternal(g);
						somethingDone = true;
					}

					for(let p of values.where(v => v.multiple != 1 && Multiple % v.multiple == 0))
					{
						this._multiple /= p.multiple;
						p.multiple = 1;
						somethingDone = true;
					}

					for(let g of values
					.skip(1).ofType(OperatorGene)
					.where(p => p.operator == DIVISION && p.count > 1)
					.take(1))
					{

						f = values.First();
						fo = f as OperatorGene;
						if (fo == null || fo.operator != MULTIPLICATION)
						{
							fo = new OperatorGene(MULTIPLICATION);

							Replace(f, fo);
							fo.Add(f);
						}

						// Here we have a classical inversion case that can help in reduction.
						for(let n of g.values.skip(1).toArray())
						{

							g.Remove(n);
							fo.Add(n);

							somethingDone = true;
						}
					}


				}
					break;
			}/* */

			var multiple = this._multiple;
			if (multiple == 0 || !isFinite(multiple) || isNaN(multiple))
			{
				if (values.any())
				{
					this._clearInternal();
					somethingDone = true;
				}
			}


			for(let o of values.ofType(OperatorGene)
			.toArray())
			{
				if (!values.Contains(o))
					continue;


				if (o.Reduce())
					somethingDone = true;

				if (o.Count == 0)
				{
					somethingDone = true;

					Replace(o, new ConstantGene(o.multiple));
					continue;
				}



				if (reduceGroupings)
				{
					if (this.operator == ADDITION)
					{
						if (o.multiple == 0)
						{
							Remove(o);
							somethingDone = true;
							continue;
						}

						if (o.operator == ADDITION)
						{
							let index = values.IndexOf(o);
							for(let og of o.values.toArray())
							{
								o.Remove(og);
								og.multiple *= o.multiple;
								values.Insert(index, og);
							}
							Remove(o);

							somethingDone = true;
							continue;
						}
					}

					if (this.operator == MULTIPLICATION)
					{

						if (o.operator == MULTIPLICATION)
						{
							this._multiple *= o._multiple;

							let index = values.IndexOf(o);
							for(let og of o.values.toArray())
							{
								o.Remove(og);
								values.Insert(index, og);
							}
							Remove(o);

							somethingDone = true;
							continue;
						}
					}
				}

				if (o.Count == 1)
				{

					var opg = o.values.FirstOrDefault();

					if (opg instanceof ParameterGene || opg instanceof ConstantGene)
					{
						if (!AvailableFunctions.Contains(o.operator))
						{

							o.Remove(opg);
							opg.multiple *= o.multiple;
							Replace(o, opg);

							somethingDone = true;
							continue;

						}
						else if (o.operator == SQUARE_ROOT && opg instanceof ConstantGene && opg.multiple >= 0)
						{
							let sq = Math.Sqrt(opg.multiple);
							for (let i = 0; i < sq; i++)
							{
								if (i == sq)
								{
									o.Remove(opg);
									Replace(o, opg);
									somethingDone = true;
								}

							}
						}
					}

					if (opg instanceof OperatorGene)
					{
						let og = <OperatorGene>opg;

						if (o.operator == SQUARE_ROOT)
						{

							let childCount = og.Children.Count;

							// Square root of square?
							if (og.operator == MULTIPLICATION && childCount==2 && og.Children.Select(p=>p.AsReduced().toString()).Distinct().Count()==1)
							{
								let children = og.Children.Cast<ParameterGene>().toArray();
								og.Remove(children[0]);
								o.operator = MULTIPLICATION;
								o.values.Clear();
								o.values.Add(children[0]);

								somethingDone = true;
							}

						}
						else if (AvailableOperators.Contains(o.operator))
						{
							let children = og.Children.toArray();
							o.operator = og.operator;
							o.multiple *= og.multiple;
							og.values.Clear();
							o.values.Clear();
							o.values.AddRange(children);

							somethingDone = true;
						}
					}


				}
			}


			if (this.operator != DIVISION)
			{
				values.Sort((a, b) =>
				{
					if (a.multiple < b.multiple)
						return 1;

					if (a.multiple > b.multiple)
						return -1;

					let ats = a.toString();
					let bts = b.toString();
					if (ats == bts)
						return 0;

					let ts = [ats, bts];
					ts.sort();

					return ts[0] == ats ? -1 : +1;

				});
			}

			if (somethingDone)
				this._onModified();

			return somethingDone;
		}

		asReduced():OperatorGene
		{

			var gene = this.clone();
			gene.reduce();
			return gene;
		}


	get children():AlgebraGene[]
	{
		return this.toArray();
	}




	toStringContents():string
		{
			if (AvailableFunctions.indexOf(this.operator)!=-1)
			{
				if (this._source.length == 1)
					return this.operator + this._source[0].toString();
				else
					return this.operator +
						parenGroup( this.valuesArranged.select(s=>s.toString()).toArray().join(","));
			}
			return parenGroup( this.valuesArranged.select(s=>s.toString()).toArray().join(this.operator));
		}


	clone():OperatorGene
		{
			var clone = new OperatorGene(this._operator,this._multiple);
			this.forEach(g=>{
				clone._addInternal(g.clone());
			});
			return clone;
		}

	static getRandomOperator(excluded?:string|IEnumerableOrArray<string>):OperatorString
		{
			if(!excluded || Type.isString(excluded))
				return getRandomFrom(AvailableOperators,excluded);

			return getRandomFromExcluding(AvailableOperators,excluded);
		}

	static getRandomFunctionOperator(excluded?:string|IEnumerableOrArray<string>):OperatorString
	{
		if(!excluded || Type.isString(excluded))
			return getRandomFrom(AvailableFunctions,excluded);

		return getRandomFromExcluding(AvailableFunctions,excluded);
	}
	static getRandomOperation(excluded?:string|IEnumerableOrArray<string>):OperatorGene
		{
			return new OperatorGene(OperatorGene.getRandomOperator(excluded));
		}

	protected calculateWithoutMultiple(values:number[]):number
		{
			var results = this._source.map(s => s.calculate(values));
			switch (OperatorString)
			{

				case ADDITION:
					return Procedure.sum(results);

				// For now "Power Of" functions will be elaborated with a*a.
				case MULTIPLICATION:
					return Procedure.product(results);

				case DIVISION:
					return Procedure.Quotient(results);

				// Single value functions...
				case SQUARE_ROOT:
					if (results.length == 1)
						return Math.sqrt(results[0]);
					break;
			}

			return NaN;
		}

	replace(target:AlgebraGene, replacement:AlgebraGene)
		{
			var index = this._source.indexOf(target);
			if (index == -1)
				throw new ArgumentOutOfRangeException("Target gene not found.");

			this.set(index,replacement);
		}

	}

export default OperatorGene;