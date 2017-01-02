/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Open.Formatting;

namespace AlgebraBlackBox.Genes
{

	public abstract class ReducibleGeneNode : GeneNode, IReducibleGene
	{

		public ReducibleGeneNode(double multiple = 1) : base(multiple)
		{

		}

		protected override void OnDispose(bool calledExplicitly)
		{
			base.OnDispose(calledExplicitly);
			_reduced = null;
		}

		override protected void OnModified()
		{
			base.OnModified();
			if (_reduced == null || _reduced.IsValueCreated)
				_reduced = Lazy.New(() => this.Clone().Reduce() ?? this); // Use a clone to prevent any threading issues.
		}

		// Having this method makes it easier to debug.
		protected IEnumerable<IGene> CloneChildren()
		{
			return GetChildren().Select(g => g.Clone());
		}

		public new ReducibleGeneNode Clone()
		{
			return (ReducibleGeneNode)CloneInternal();
		}

		Lazy<IGene> _reduced;

		public bool IsReducible
		{
			get
			{
				var r = _reduced;
				AssertIsLiving();
				return r.Value != this;
			}
		}

		public IGene AsReduced(bool ensureClone = false)
		{
			var r = _reduced;
			AssertIsLiving();
			var v = r.Value;
			return ensureClone ? v.Clone() : v;
		}

		static readonly double[] QuickCheck = new double[] {1,1};
		protected IGene ChildReduce(IReducibleGene child)
		{
			// Here's the magic... If the Reduce call returns non-null, then attempt to replace o with (new) g.
			#if DEBUG
			var cValue = child.Calculate(QuickCheck);
			var cString = child.ToString();
			var cCheck = child.ToStringContents();
			#endif
			var g = child.Reduce();
			if (g != null)
			{
				#if DEBUG
				var gValue = g.Calculate(QuickCheck);
				var gStrign = g.ToString();
				var gCheck = g.ToStringContents();
				Debug.Assert(double.IsNaN(cValue) || cValue.IsRelativeNearEqual(gValue,7));
				#endif
				if (!ReplaceChild(child, g))
					Sync.Poke(); // Child may have changed internally.
			}
			return g;
		}

		protected virtual IGene ReplaceWithReduced()
		{
			return GetChildren().Count == 0 ? new ConstantGene(Multiple) : null;
		}


		/*
            REDUCTION:
            Goal, avoid reference to parent so that nodes can be reused across genes.
            When calling .Reduce() a gene will be returned as a gene that could replace this one,
            or the same gene is returned signaling that it's contents were updated.
        */

		static bool MultipleIsExtreme(double m)
		{
			return m == 0 || double.IsInfinity(m) || double.IsNaN(m);
		}

		public IGene Reduce()
		{
			var m = Multiple;
			if (MultipleIsExtreme(m))
				return new ConstantGene(m);

			var reduced = this;
			var sync = Sync;
			var modified = sync.Modifying(() =>
			{
				var modCount = 0;
				// Inner loop will keep going while changes are still occuring.
				while (!MultipleIsExtreme(Multiple) && sync.Modifying(() =>
				{
					foreach (var o in GetChildren().OfType<IReducibleGene>().ToArray())
					{
						if (ChildReduce(o) != null)
							modCount++;
					}

					ReduceLoop();
				}))
				{
					modCount++;
					if(modCount>1000)
					{
						if(Debugger.IsAttached)
							Debugger.Break();
						else {
							var message = "Potential infinite reduction loop in "+this.GetType();
							Console.WriteLine("");
							Console.WriteLine("==========================================================");
							Console.WriteLine(message);
							throw new Exception(message);
						}
					}
				}
				return modCount != 0;
			});

			// Last round check...
			m = Multiple;
			if (MultipleIsExtreme(m))
				return new ConstantGene(m);

			return ReplaceWithReduced() ??
				(modified ? reduced : null);
		}



		// Call this at the end of the sub-classes reduce loop.
		protected abstract void ReduceLoop();

	}

	public abstract class ReducibleGene : Gene, IReducibleGene
	{

		public ReducibleGene(double multiple = 1) : base(multiple)
		{

		}

		protected override void OnDispose(bool calledExplicitly)
		{
			base.OnDispose(calledExplicitly);
			_reduced = null;
		}

		override protected void OnModified()
		{
			base.OnModified();
			if (_reduced == null || _reduced.IsValueCreated)
				_reduced = Lazy.New(() => this.Clone().Reduce() ?? this); // Use a clone to prevent any threading issues.
		}

		Lazy<IGene> _reduced;

		public bool IsReducible
		{
			get
			{
				var r = _reduced;
				AssertIsLiving();
				return r.Value != this;
			}
		}

		public new ReducibleGene Clone()
		{
			return (ReducibleGene)CloneInternal();
		}


		public IGene AsReduced(bool ensureClone = false)
		{
			var r = _reduced;
			AssertIsLiving();
			var v = r.Value;
			return ensureClone ? v.Clone() : v;
		}
		public virtual IGene Reduce()
		{
			var m = Multiple;
			if (m == 0
			|| double.IsInfinity(m)
			|| double.IsNaN(m))
				return new ConstantGene(m);

			return null;
		}
	}

}
