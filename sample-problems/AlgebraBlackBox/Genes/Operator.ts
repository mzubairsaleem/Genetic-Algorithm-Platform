///<reference path="../../../node_modules/typescript-dotnet/source/System/FunctionTypes"/>

import * as Enumerable from "../../../node_modules/typescript-dotnet/source/System.Linq/Linq";
import AlgebraGene from "../Gene";
import ConstantGene from "./ConstantGene";

const ADDITION:string = "+", // Subtraction is simply a negative multiplier.
      MULTIPLICATION:string = "*",
      DIVISION:string = "/",
      SQUARE_ROOT:string = "√";

type OperatorString = ADDITION | MULTIPLICATION | DIVISION | SQUARE_ROOT;

function arrange(a:AlgebraGene, b:AlgebraGene):CompareResult {
	return b.multiple > a.multiple || b.toString() > a.toString();
}

class OperatorGene extends AlgebraGene
{

	private _operator:OperatorString;
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
					var pa = p.memoize();

					switch (OperatorString)
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
			

			switch (OperatorString)
			{
				case MULTIPLICATION:
				{
					if (values.any(v => !v.multiple))
					{
						_.multiple = 0;
						somethingDone = true;
						break;
					}

					var constGenes = values
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


					var mParams = values
						.ofType(ParameterGene)
						.memoize();

					if (mParams.any())
					{

						var divOperator = values
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
							var oneToKill = divOperator.Values
								.skip(1)
								.ofType(ParameterGene)
							.where(p => mParams.any(pn => pn.id == p.id))
							.first();

							var mPToKill = mParams.first(p => p.hash == oneToKill.hash);

							divOperator.replace(oneToKill, new ConstantGene(oneToKill.Multiple));
							this.replace(mPToKill, new ConstantGene(mPToKill.Multiple));

							somethingDone = true;
						}
					}


				}
					break;
				case DIVISION:
				{
					var f = Values.FirstOrDefault();
					if (f != null && f.Multiple != 1)
					{
						Multiple *= f.Multiple;
						f.Multiple = 1;
						somethingDone = true;
						break;
					}

					foreach (var g in Values.Take(1).where(v => v.Multiple == 0))
					{
						Multiple = 0;
						somethingDone = true;
					}

					foreach (var g in Values.skip(1).where(v => v.Multiple == 0))
					{
						Multiple = double.NaN;  // It is possible to specify positive or negative infinity...
						somethingDone = true;
					}

					if (Multiple == 0 || double.IsNaN(Multiple))
						break;

					foreach (var p in Values.where(v => v.Multiple < 0))
					{
						somethingDone = true;
						this.Multiple *= -1;
						p.Multiple *= -1;
					}

					var newConst = Values.LastOrDefault() as ConstantGene;
					var havMultiples = Values.skip(1).where(v => v.Multiple != 1d && v!=newConst).ToArray();
					if(havMultiples.any()){

						if (newConst == null) {
							newConst = new ConstantGene(1d);
							Values.Add(newConst);
						}

						foreach (var g in havMultiples)
						{
							newConst.Multiple *= g.Multiple;
							g.Multiple = 1d;
							if (g is ConstantGene)
							Remove(g);
						}

						somethingDone = true;
					}


					var fo = f as OperatorGene;

					if (fo != null)
					{
						IEnumerable<Gene> msource = null;

						switch (fo.operator)
						{
							case MULTIPLICATION:
								msource = fo.Values;
								break;
							case DIVISION:
								msource = fo.Values.Take(1);
								break;
							default:
								msource = Enumerable.Empty<Gene>();
								break;
						}

						var mParams = msource
							.ofType(ParameterGene)
						.ToArray();

						if (mParams.any())
						{
							var oneToKill = Values
								.ofType(ParameterGene)
							.where(p => mParams.any(pn => pn.id == p.id))
							.FirstOrDefault();

							if (oneToKill != null)
							{
								var mPToKill = mParams.First(p => p.id == oneToKill.id);
								Replace(oneToKill, new ConstantGene(oneToKill.Multiple));
								fo.Replace(mPToKill, new ConstantGene(mPToKill.Multiple));

								somethingDone = true;
							}
						}

					}

					var fp = f as ParameterGene;

					if (fp != null)
					{
						var multOperator = Values
							.ofType(OperatorGene)
						.where(g => g.operator == MULTIPLICATION
						&& g.Values
							.ofType(ParameterGene)
						.any(g2 => g2.id == fp.id))
					.FirstOrDefault();

						if (multOperator != null)
						{
							var oneToKill = multOperator.Values
								.ofType(ParameterGene)
							.where(p => p.id == fp.id)
							.First();

							Replace(fp, new ConstantGene(fp.Multiple));
							multOperator.Replace(oneToKill, new ConstantGene(oneToKill.Multiple));

							somethingDone = true;
						}
						else
						{
							var divOperator = Values
								.ofType(OperatorGene)
							.where(g => g.operator == DIVISION
							&& g.Values.Take(1)
								.ofType(ParameterGene)
							.any(g2 => g2.id == fp.id))
						.FirstOrDefault();

							if (divOperator != null)
							{
								var oneToKill = (ParameterGene)divOperator.Values.First();


								Replace(fp, new ConstantGene(fp.Multiple));
								divOperator.Replace(oneToKill, new ConstantGene(oneToKill.Multiple));

								somethingDone = true;
							}
						}


					}

					foreach (var g in Values.skip(1).where(p => p is ConstantGene && Multiple % p.Multiple == 0).ToArray())
					{
						Multiple /= g.Multiple;
						Values.Remove(g);
						somethingDone = true;
					}

					foreach (var p in Values.where(v => v.Multiple != 1 && Multiple % v.Multiple == 0))
					{
						Multiple /= p.Multiple;
						p.Multiple = 1;
						somethingDone = true;
					}

					foreach (var g in Values
					.skip(1).ofType(OperatorGene)
					.where(p => p.operator == DIVISION && p.Count > 1)
					.Take(1))
					{

						f = Values.First();
						fo = f as OperatorGene;
						if (fo == null || fo.operator != MULTIPLICATION)
						{
							fo = new OperatorGene() { OperatorString = MULTIPLICATION };

							Replace(f, fo);
							fo.Add(f);
						}

						// Here we have a classical inversion case that can help in reduction.
						foreach (var n in g.Values.skip(1).ToArray())
						{

							g.Remove(n);
							fo.Add(n);

							somethingDone = true;
						}
					}


				}
					break;
			}/* */

			if (Multiple == 0 || double.IsInfinity(Multiple) || double.IsNaN(Multiple))
			{
				if (Values.any())
				{
					Values.Clear();
					somethingDone = true;
				}
			}


			foreach (var o in Values.ofType(OperatorGene)
			.ToArray())
			{
				if (!Values.Contains(o))
					continue;


				if (o.Reduce())
					somethingDone = true;

				if (o.Count == 0)
				{
					somethingDone = true;

					Replace(o, new ConstantGene(o.Multiple));
					continue;
				}



				if (reduceGroupings)
				{
					if (OperatorString == ADDITION)
					{
						if (o.Multiple == 0)
						{
							Remove(o);
							somethingDone = true;
							continue;
						}

						if (o.operator == ADDITION)
						{
							var index = Values.IndexOf(o);
							foreach (var og in o.Values.ToArray())
							{
								o.Remove(og);
								og.Multiple *= o.Multiple;
								Values.Insert(index, og);
							}
							Remove(o);

							somethingDone = true;
							continue;
						}
					}

					if (OperatorString == MULTIPLICATION)
					{

						if (o.operator == MULTIPLICATION)
						{
							Multiple *= o.Multiple;

							var index = Values.IndexOf(o);
							foreach (var og in o.Values.ToArray())
							{
								o.Remove(og);
								Values.Insert(index, og);
							}
							Remove(o);

							somethingDone = true;
							continue;
						}
					}
				}

				if (o.Count == 1)
				{

					Gene opg = o.Values.FirstOrDefault();

					if (opg instanceof ParameterGene || opg instanceof ConstantGene)
					{
						if (!AvailableFunctions.Contains(o.operator))
						{

							o.Remove(opg);
							opg.Multiple *= o.Multiple;
							Replace(o, opg);

							somethingDone = true;
							continue;

						}
						else if (o.operator == SQUARE_ROOT && opg instanceof ConstantGene && opg.Multiple >= 0)
						{
							var sq = Math.Sqrt(opg.Multiple);
							for (int i = 0; i < sq; i++)
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
						var og = (OperatorGene)opg;

						if (o.operator == SQUARE_ROOT)
						{

							var childCount = og.Children.Count;

							// Squareroot of square?
							if (og.operator == MULTIPLICATION && childCount==2 && og.Children.Select(p=>p.AsReduced().ToString()).Distinct().Count()==1)
							{
								var children = og.Children.Cast<ParameterGene>().ToArray();
								og.Remove(children[0]);
								o.operator = MULTIPLICATION;
								o.Values.Clear();
								o.Values.Add(children[0]);

								somethingDone = true;
							}

						}
						else if (AvailableOperators.Contains(o.operator))
						{
							var children = og.Children.ToArray();
							o.operator = og.operator;
							o.Multiple *= og.Multiple;
							og.Values.Clear();
							o.Values.Clear();
							o.Values.AddRange(children);

							somethingDone = true;
						}
					}


				}
			}


			if (OperatorString != DIVISION)
			{
				Values.Sort((a, b) =>
				{
					if (a.Multiple < b.Multiple)
						return 1;

					if (a.Multiple > b.Multiple)
						return -1;

					var ats = a.ToString();
					var bts = b.ToString();
					if (ats == bts)
						return 0;

					var ts = new string[2] { ats, bts };
					Array.Sort(ts);

					return ts[0] == ats ? -1 : +1;

				});
			}

			if (somethingDone)
				ResetToString();

			return somethingDone;
		}

	public override Gene AsReduced()
		{

			var gene = (OperatorGene)this.Clone();
			gene.Reduce();
			return gene;

		}


	public override IReadOnlyCollection<Gene> Children
		{
			get
			{
				return ValuesReadOnly;
			}
		}

		char _operator;
	public char OperatorString
		{
			get { return _operator; }
			set
			{
				_operator = value;
				ResetToString();
			}
		}

	public override string ToStringContents()
		{
			if (AvailableFunctions.Contains(OperatorString))
			{
				if (Values.Count == 1)
					return OperatorString + Values.Single().ToString();
				else
					return OperatorString + "(" + String.Join(",", ValuesArranged.Select(s => s.ToString())) + ")";
			}
			return "(" + String.Join(OperatorString.ToString(), ValuesArranged.Select(s => s.ToString())) + ")";
		}


	public override Gene Clone()
		{
			var clone = new OperatorGene()
			{
				OperatorString = OperatorString,
					Multiple = Multiple
			};

			foreach (var g in Values)
			clone.Values.Add(g.Clone());

			return clone;
		}

	public static readonly char[] AvailableOperators = new char[] { ADDITION, MULTIPLICATION, DIVISION };
	public static readonly char[] AvailableFunctions = new char[] { SQUARE_ROOT };

	public static char GetRandomOperator(IEnumerable<char> excluded = null)
		{
			var ao = excluded == null
				? AvailableOperators
				: AvailableOperators.where(o => !excluded.Contains(o)).ToArray();
			return ao[Environment.Randomizer.Next(ao.Length)];
		}

	public static char GetRandomOperator(char excluded)
		{
			var ao = AvailableOperators.where(o => o != excluded).ToArray();
			return ao[Environment.Randomizer.Next(ao.Length)];
		}

	public static char GetRandomFunctionOperator()
		{
			return AvailableFunctions[Environment.Randomizer.Next(AvailableFunctions.Length)];
		}

	public static OperatorGene GetRandomOperation(IEnumerable<char> excluded = null)
		{
			return new OperatorGene() { OperatorString = GetRandomOperator(excluded) };
		}

	public static OperatorGene GetRandomOperation(char excluded)
		{
			return new OperatorGene() { OperatorString = GetRandomOperator(excluded) };
		}

	public static OperatorGene GetRandomFunction()
		{
			return new OperatorGene() { OperatorString = GetRandomFunctionOperator() };
		}

	protected override double CalculateWithoutMultiple(double[] values)
		{
			var results = Values.Select(s => s.Calculate(values)).ToArray();
			switch (OperatorString)
			{

				case ADDITION:
					return results.any() ? results.Sum() : 0;

				// For now "Power Of" functions will be elaborated with a*a.
				case MULTIPLICATION:
					return results.Product();

				case DIVISION:
					return results.Quotient();

				// Single value functions...
				case SQUARE_ROOT:
					if (results.Length != 1)
						return double.NaN;

					return Math.Sqrt(results[0]);
			}

			return double.NaN;
		}

	public override void Replace(Gene target, Gene replacement)
		{
			var index = Values.IndexOf(target);
			if (index == -1)
				throw new ArgumentOutOfRangeException("Target gene not found.");

			Values.Insert(index, replacement);
			Values.Remove(target);
			ResetToString();
		}

	}

export default OperatorGene;