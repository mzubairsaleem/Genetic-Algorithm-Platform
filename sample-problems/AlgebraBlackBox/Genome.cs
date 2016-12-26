using System;
using System.Threading.Tasks;
using Open.Threading;

namespace AlgebraBlackBox
{
	public class Genome : GeneticAlgorithmPlatform.Genome<IGene>, IEquatable<Genome>, IReducible<Genome>
	{
		public Genome(IGene root) : base(root)
		{

		}


		protected override void OnModified()
		{
			base.OnModified();
			if (_reduced == null || _reduced.IsValueCreated)
				_reduced = Lazy.New(() =>
				{
					var r = Root as IReducibleGene;
					return r != null && r.IsReducible ? new Genome(r.AsReduced()) : this;
				}); // Use a clone to prevent any threading issues.
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

		public Task<double> Calculate(double[] values)
		{
			return Root.Calculate(values);
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
			return ensureClone ? r.Clone() : r;
		}


		// ConcurrentDictionary<string, Genome> _variations;
		// public IEnumerable<Genome> Variations
		// {
		// 	get
		// 	{
		// 		return _variations.Values;
		// 	}
		// }

		// public bool RegisterVariation(Genome v)
		// {
		// 	return _variations.TryAdd(v.Hash, v);
		// }

	}
}
