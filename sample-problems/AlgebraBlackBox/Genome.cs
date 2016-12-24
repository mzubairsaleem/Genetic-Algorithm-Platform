using System;
using System.Threading.Tasks;

namespace AlgebraBlackBox
{
    public class Genome : GeneticAlgorithmPlatform.Genome<IGene>, IEquatable<Genome>
	{
		Lazy<string> _cachedToString;
		Lazy<string> _cachedToStringReduced;

		public Genome(IGene root):base(root)
		{
		}

		public override void ResetHash()
		{
			base.ResetHash();
			_cachedToString = new Lazy<string>(ToString);
			_cachedToStringReduced = new Lazy<string>(ToStringReduced);
		}

		public new IGeneNode FindParent(IGene child)
        {
			return (IGeneNode)base.FindParent(child);
        }

		public void Replace(IGene target, IGene replacment)
		{
 			if (Root==target)
                Root = replacment;
            else 
			{
                var parent = FindParent(target);
				if(parent==null)
					throw new ArgumentException("'target' not found.");
				parent.Replace(target, replacment);			
			}
		}

		public override string ToString()
		{
			return Root.ToString().Replace("+-","-");
		}

		public string ToStringReduced()
		{
			return Root.AsReduced().ToString().Replace("+-", "-");
		}

		public string CachedToString
		{
			get { return _cachedToString.Value; }
		}

		public string CachedToStringReduced
		{
			get { return _cachedToStringReduced.Value; }
		}

		public new Genome Clone()
		{
			return new Genome(Root.Clone());
		}

		public Task<double> Calculate(double[] values)
		{
			return Root.Calculate(values);
		}

		public Genome AsReduced()
		{
			return Root.IsReducible() ? new Genome( Root.AsReduced(true) ) : this;
		}

        public string ToAlphaParameters(bool reduced = false)
        {
            return AlphaParameters.ConvertTo(reduced ? CachedToStringReduced : CachedToString);
        }



		#region IEquatable<Genome> Members

		public bool Equals(Genome other)
		{
			return this.CachedToStringReduced==other.CachedToStringReduced;
		}

		#endregion


	}
}
