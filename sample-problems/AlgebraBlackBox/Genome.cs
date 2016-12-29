using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Open.Collections;
using Open.Threading;

namespace AlgebraBlackBox
{
	public class Genome : GeneticAlgorithmPlatform.Genome<IGene>, IEquatable<Genome>, IReducible<Genome>
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
					// Be careful not to call .IsReducible in here as it may recurse.
					var root = r.AsReduced();
					if (root == Root) return this;
					var g = new Genome(root);
					g.Freeze();

					return g;
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

		public double Calculate(double[] values)
		{
			return Root.Calculate(values);
		}

		public Task<double> CalculateAsync(double[] values)
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


		public override GeneticAlgorithmPlatform.IGenome NextVariation()
		{
			var source = _variations;
			if (source == null) return null;
			var e = LazyInitializer.EnsureInitialized(ref _varationEnumerator, () => source.GetEnumerator());
			if (e.MoveNext()) return e.Current;
			Interlocked.Exchange(ref _varationEnumerator, null);
			return NextVariation();
		}

		IEnumerator<Genome> _varationEnumerator;
		LazyList<Genome> _variations;
		public IReadOnlyList<Genome> Variations
		{
			get
			{
				return _variations;
			}
		}

		public void RegisterVariations(IEnumerable<Genome> variations)
		{
			if (this.IsReadOnly && _variations != null)
				throw new ArgumentException("Cannot register variations after frozen.", "variations");
			_variations = variations.Memoize();
		}

	}
}
