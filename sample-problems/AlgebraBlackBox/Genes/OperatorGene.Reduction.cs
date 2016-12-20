using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgebraBlackBox.Genes
{
    public partial class OperatorGene
	{

		public bool Reduce(bool reduceGroupings = false)
		{
			bool somethingdone = false;
			while (ReduceLoop(reduceGroupings)) { somethingdone = true; }
			return somethingdone;
		}

		protected bool ReduceLoop(bool reduceGroupings = false)
		{

			bool somethingDone = false;


			foreach (var p in Values
				.GroupBy(g => g.ToStringContents())
				.Where(g => g.Count() > 1))
			{
				var pa = p.ToArray();


				switch (Operator)
				{
					case '+':
						{
							var sum = pa.Sum(s => s.Multiple);
							var hero = pa.First();

							if (sum == 0)
								Replace(hero, new ConstantGene(0));
							else
								hero.Multiple = sum;

							foreach (var g in pa.Skip(1))
								Values.Remove(g);

							somethingDone = true;
						}
						break;

					case '/':
						{
							var first = Values.First();
							if (!(first is ConstantGene) && first.ToStringContents() == pa[0].ToStringContents())
							{
								Replace(first, new ConstantGene(first.Multiple));
								Replace(pa[1], new ConstantGene(pa[1].Multiple));
								somethingDone = true;
							}
						}
						break;

				}

			}/* */




			switch (Operator)
			{
				case '*':
					{
						if (Values.Any(v => v.Multiple == 0))
						{
							Multiple = 0;
							somethingDone = true;
							break;
						}


						var constGenes = Values
							.Where(p => p is ConstantGene)
							.Cast<ConstantGene>()
							.ToArray();

						if (Values.Count > 1 && constGenes.Any())
						{
							foreach (var g in constGenes)
							{
								Values.Remove(g);
								this.Multiple *= g.Multiple;
								somethingDone = true;
							}

							if (!Values.Any())
							{
								Values.Add(new ConstantGene(this.Multiple));
								this.Multiple = 1;
								somethingDone = true;
							}
						}

						foreach (var p in Values.Where(v => v.Multiple != 1))
						{
							somethingDone = true;
							this.Multiple *= p.Multiple;
							p.Multiple = 1;
						}

						var mParams = Values
							.Where(p => p is ParameterGene)
							.Cast<ParameterGene>()
							.ToArray();

						if (mParams.Any())
						{

							var divOperator = Values
								.Where(g => g is OperatorGene)
								.Cast<OperatorGene>()
								.Where(g => g.Operator == '/'
									&& g.Values
									.Skip(1)
									.Where(g2 => g2 is ParameterGene)
									.Cast<ParameterGene>()
									.Any(g2 => mParams.Any(g3 => g2.ID == g3.ID)))
								.FirstOrDefault();

							if (divOperator != null)
							{
								var oneToKill = divOperator.Values
									.Skip(1)
									.Where(p => p is ParameterGene)
									.Cast<ParameterGene>()
									.Where(p => mParams.Any(pn => pn.ID == p.ID))
									.First();

								var mPToKill = mParams.First(p => p.ID == oneToKill.ID);

								divOperator.Replace(oneToKill, new ConstantGene(oneToKill.Multiple));
								Replace(mPToKill, new ConstantGene(mPToKill.Multiple));

								somethingDone = true;
							}
						}


					}
					break;
				case '/':
					{
						var f = Values.FirstOrDefault();
						if (f != null && f.Multiple != 1)
						{
							Multiple *= f.Multiple;
							f.Multiple = 1;
							somethingDone = true;
							break;
						}

						foreach (var g in Values.Take(1).Where(v => v.Multiple == 0))
						{
							Multiple = 0;
							somethingDone = true;
						}

						foreach (var g in Values.Skip(1).Where(v => v.Multiple == 0))
						{
							Multiple = double.NaN;  // It is possible to specify positive or negative infinity...
							somethingDone = true;
						}

						if (Multiple == 0 || double.IsNaN(Multiple))
							break;

						foreach (var p in Values.Where(v => v.Multiple < 0))
						{
							somethingDone = true;
							this.Multiple *= -1;
							p.Multiple *= -1;
						}

						var newConst = Values.LastOrDefault() as ConstantGene;
						var havMultiples = Values.Skip(1).Where(v => v.Multiple != 1d && v!=newConst).ToArray();
						if(havMultiples.Any()){

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

							switch (fo.Operator)
							{
								case '*':
									msource = fo.Values;
									break;
								case '/':
									msource = fo.Values.Take(1);
									break;
								default:
									msource = Enumerable.Empty<Gene>();
									break;
							}

							var mParams = msource
								.Where(p => p is ParameterGene)
								.Cast<ParameterGene>()
								.ToArray();

							if (mParams.Any())
							{
								var oneToKill = Values
									.Where(p => p is ParameterGene)
									.Cast<ParameterGene>()
									.Where(p => mParams.Any(pn => pn.ID == p.ID))
									.FirstOrDefault();

								if (oneToKill != null)
								{
									var mPToKill = mParams.First(p => p.ID == oneToKill.ID);
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
								.Where(g => g is OperatorGene)
								.Cast<OperatorGene>()
								.Where(g => g.Operator == '*'
									&& g.Values
									.Where(g2 => g2 is ParameterGene)
									.Cast<ParameterGene>()
									.Any(g2 => g2.ID == fp.ID))
								.FirstOrDefault();

							if (multOperator != null)
							{
								var oneToKill = multOperator.Values
									.Where(p => p is ParameterGene)
									.Cast<ParameterGene>()
									.Where(p => p.ID == fp.ID)
									.First();

								Replace(fp, new ConstantGene(fp.Multiple));
								multOperator.Replace(oneToKill, new ConstantGene(oneToKill.Multiple));

								somethingDone = true;
							}
							else
							{
								var divOperator = Values
									.Where(g => g is OperatorGene)
									.Cast<OperatorGene>()
									.Where(g => g.Operator == '/'
										&& g.Values.Take(1)
										.Where(g2 => g2 is ParameterGene)
										.Cast<ParameterGene>()
										.Any(g2 => g2.ID == fp.ID))
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

						foreach (var g in Values.Skip(1).Where(p => p is ConstantGene && Multiple % p.Multiple == 0).ToArray())
						{
							Multiple /= g.Multiple;
							Values.Remove(g);
							somethingDone = true;
						}

						foreach (var p in Values.Where(v => v.Multiple != 1 && Multiple % v.Multiple == 0))
						{
							Multiple /= p.Multiple;
							p.Multiple = 1;
							somethingDone = true;
						}

						foreach (var g in Values
							.Skip(1)
							.Where(p => p is OperatorGene)
							.Cast<OperatorGene>()
							.Where(p => p.Operator == '/' && p.Count > 1)
							.Take(1))
						{

							f = Values.First();
							fo = f as OperatorGene;
							if (fo == null || fo.Operator != '*')
							{
								fo = new OperatorGene() { Operator = '*' };

								Replace(f, fo);
								fo.Add(f);
							}

							// Here we have a classical inversion case that can help in reduction.
							foreach (var n in g.Values.Skip(1).ToArray())
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
				if (Values.Any())
				{
					Values.Clear();
					somethingDone = true;
				}
			}


			foreach (var o in Values
				.Where(p => p is OperatorGene)
				.Cast<OperatorGene>()
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
					if (Operator == '+')
					{
						if (o.Multiple == 0)
						{
							Remove(o);
							somethingDone = true;
							continue;
						}

						if (o.Operator == '+')
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

					if (Operator == '*')
					{

						if (o.Operator == '*')
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

					if (opg is ParameterGene || opg is ConstantGene)
					{
						if (!AvailableFunctions.Contains(o.Operator))
						{

							o.Remove(opg);
							opg.Multiple *= o.Multiple;
							Replace(o, opg);

							somethingDone = true;
							continue;

						}
						else if (o.Operator == '√' && opg is ConstantGene && opg.Multiple >= 0)
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

					if (opg is OperatorGene)
					{
						var og = (OperatorGene)opg;

						if (o.Operator == '√')
						{

							var childCount = og.Children.Count;

							// Squareroot of square?
							if (og.Operator == '*' && childCount==2 && og.Children.Select(p=>p.AsReduced().ToString()).Distinct().Count()==1)
							{
								var children = og.Children.Cast<ParameterGene>().ToArray();
								og.Remove(children[0]);
								o.Operator = '*';
								o.Values.Clear();
								o.Values.Add(children[0]);

								somethingDone = true;
							}

						}
						else if (AvailableOperators.Contains(o.Operator))
						{
							var children = og.Children.ToArray();
							o.Operator = og.Operator;
							o.Multiple *= og.Multiple;
							og.Values.Clear();
							o.Values.Clear();
							o.Values.AddRange(children);

							somethingDone = true;
						}
					}


				}
			}


			if (Operator != '/')
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


    }
}