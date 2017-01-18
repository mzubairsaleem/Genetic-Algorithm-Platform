using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AlgebraBlackBox.Genes;
using GeneticAlgorithmPlatform;
using Open.Collections;
using Open.Threading;

namespace AlgebraBlackBox
{
	public class Genome : GeneticAlgorithmPlatform.GenomeBase<IGene>, IEquatable<Genome>, IReducible<Genome>
	{
		public Genome(IGene root) : base(root)
		{

		}

		protected override void OnDispose(bool calledExplicitly)
		{
			base.OnDispose(calledExplicitly);
			_variations = null;
		}



		protected override void OnModified()
		{
			base.OnModified();
			if (_reduced == null || _reduced.IsValueCreated)
				_reduced = Lazy.New(() =>
				{
					var r = Root as IReducibleGene;
					if (r == null) return this;
					var p1 = r as ProductGene;
					//					Debug.Assert(p1==null || p1.Count>1);
					// Be careful not to call .IsReducible in here as it may recurse.
					// using (TimeoutHandler.New(2000, ms =>
					// {
					// 	Console.WriteLine("Warning: {0}.AsReduced() is taking longer than {1} milliseconds.\n", r, ms);
					// }))
					// {
					var root = r.AsReduced();
					var p2 = root as ProductGene;
					if (p2 != null && p2.Count < 2)
					{
						Debugger.Break();
					}

					if (root == Root) return this;
					var g = new Genome(root);
					g.Freeze();

					return g;
					// }
				}); // Use a clone to prevent any threading issues.
			_variations = null;
		}


		protected override void OnRootChanged(IGene oldRoot, IGene newRoot)
		{
			if (newRoot != null)
			{
				var newM = newRoot.Sync as ModificationSynchronizer;
				if (newM != null)
					newM.Modified += OnModified;
			}
			if (oldRoot != null)
			{
				var oldM = oldRoot.Sync as ModificationSynchronizer;
				if (oldM != null)
					oldM.Modified -= OnModified;
			}
		}

		public new IGeneNode FindParent(IGene child)
		{
			return (IGeneNode)base.FindParent(child);
		}

		public bool Replace(IGene target, IGene replacement)
		{
			AssertIsLiving();
			if (target != replacement)
			{
				return Sync.Modifying(
					AssertIsLiving,
					() =>
					{
						if (Root == target)
						{
							return SetRoot(replacement);
						}
						else
						{
							var parent = FindParent(target);
							if (parent == null)
								throw new ArgumentException("'target' not found.");
							return parent.ReplaceChild(target, replacement);
						}
					});
			}
			return false;
		}

		public override string ToString()
		{
			return Root.ToString();
		}



		public new Genome Clone()
		{
			return new Genome(Root.Clone());
		}

		public double Calculate(IReadOnlyList<double> values)
		{
			return Root.Calculate(values);
		}

		public Task<double> CalculateAsync(IReadOnlyList<double> values)
		{
			return Root.CalculateAsync(values);
		}



		public string ToAlphaParameters(bool reduced = false)
		{
			return AlphaParameters.ConvertTo(reduced ? AsReduced().ToString() : ToString());
		}



		#region IEquatable<Genome> Members

		public bool Equals(Genome other)
		{
			return this == other || this.ToString() == other.ToString();
		}

		#endregion

		Lazy<Genome> _reduced;

		public bool IsReducible
		{
			get
			{
				return _reduced.Value != this;
			}
		}

		public Genome AsReduced(bool ensureClone = false)
		{

			var r = _reduced.Value;
			if (ensureClone)
			{
				r = r.Clone();
				r.Freeze();
			}
			return r;
		}

		internal void ReplaceReduced(Genome reduced)
		{
			if (AsReduced() != reduced)
				Interlocked.Exchange(ref _reduced, Lazy.New(() => reduced));
		}

		public override IGenome NextVariation()
		{
			var source = _variations;
			if (source == null) return null;
			var e = LazyInitializer.EnsureInitialized(ref _variationEnumerator, () => source.GetEnumerator());
			IGenome result;
			e.ConcurrentTryMoveNext(out result);
			return result;
		}

		IEnumerator<Genome> _mutationEnumerator;
		public override IGenome NextMutation()
		{
			var source = _mutationEnumerator;
			if (source == null) return null;
			IGenome result;
			source.ConcurrentTryMoveNext(out result);
			return result;
		}

		IEnumerator<Genome> _variationEnumerator;
		LazyList<Genome> _variations;
		public override IReadOnlyList<IGenome> Variations
		{
			get
			{
				return _variations;
			}
		}

		internal void RegisterVariations(IEnumerable<Genome> variations)
		{
			_variations = variations.Memoize();
		}

		internal void RegisterMutations(IEnumerable<Genome> mutations)
		{
			_mutationEnumerator = mutations.GetEnumerator();
		}

		ConcurrentHashSet<string> Expansions = new ConcurrentHashSet<string>();
		Lazy<string[]> Expansions_Array;
		public bool RegisterExpansion(string genomeHash)
		{
			var added = Expansions.Add(genomeHash);
			if (added)
				Expansions_Array = Lazy.New(() => Expansions.ToArrayDirect());
			return added;
		}

		public string[] GetExpansions()
		{
			return Expansions_Array?.Value;
		}

		// public Task<double> Correlation(
		// 	double[] aSample, double[] bSample,
		// 	Genome gA, Genome gB)
		// {
		// 	return Task.Run(() =>
		// 	{
		// 		var len = aSample.Length * bSample.Length;

		// 		var gA_result = new double[len];
		// 		var gB_result = new double[len];
		// 		var i = 0;

		// 		foreach (var a in aSample)
		// 		{
		// 			foreach (var b in bSample)
		// 			{
		// 				var p = new double[] { a, b }; // Could be using Tuples?
		// 				var r1 = gA.Calculate(p);
		// 				var r2 = gB.Calculate(p);
		// 				Task.WaitAll(r1, r2);
		// 				gA_result[i] = r1.Result;
		// 				gB_result[i] = r2.Result;
		// 				i++;
		// 			}
		// 		}

		// 		return gA_result.Correlation(gB_result);
		// 	});
		// }


		// // compare(a:AlgebraGenome, b:AlgebraGenome):boolean
		// // {
		// // 	return this.correlation(this.sample(), this.sample(), a, b)>0.9999999;
		// // }


		// public class EqualityComparer : IEqualityComparer<Genome>
		// {
		//     public bool Equals(Genome x, Genome y)
		//     {
		//         return x == y || x.Hash == y.Hash;
		//     }

		//     public int GetHashCode(Genome obj)
		//     {
		//         throw new NotImplementedException();
		//     }
		// }

	}
}
