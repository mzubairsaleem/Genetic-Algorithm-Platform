using System;

namespace AlgebraBlackBox
{
    public class Genome : GeneticAlgorithmPlatform.Genome<Gene>, IEquatable<Genome>
	{
		Lazy<string> _cachedToString;
		Lazy<string> _cachedToStringReduced;

		public Genome(Gene root):base()
		{
			_cachedToString = new Lazy<string>(ToString);
			_cachedToStringReduced = new Lazy<string>(ToStringReduced);
			Root = root;
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
			return new Genome((Gene)Root.Clone());
		}

		#region IEquatable<Genome> Members

		public bool Equals(Genome other)
		{
			return this.ToString()==other.ToString();
		}

		#endregion


	}
}
