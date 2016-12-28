
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Open.Arithmetic;
using Open.Collections;

namespace GeneticAlgorithmPlatform
{

	public class SingleFitness
	// : IComparable<SingleFitness>
	{
		ProcedureResult _result;
		object _sync = new Object();
		public SingleFitness(IEnumerable<double> scores = null) : base()
		{
			if (scores != null)
				Add(scores);
		}

		public ProcedureResult Result
		{
			get { return _result; }
		}


		public void Add(double value, int count = 1)
		{
			if (count != 0)
			{
				// Ensures 1 update at a time.
				lock (_sync) _result = _result.Add(value, count);
			}
		}

		public void Add(IEnumerable<double> values)
		{
			double sum = 0;
			int count = 0;
			foreach (var value in values)
			{
				sum += value;
				count++;
			}
			Add(sum, count);
		}

		// public int CompareTo(SingleFitness b)
		// {
		// 	if (this == b) return 0;
		// 	if (b == null)
		// 		throw new ArgumentNullException("other");
			
		// 	return _result.CompareTo(b._result);
		// }

	}

	public class Fitness : ThreadSafeTrackedList<SingleFitness>, IComparable<Fitness>
	{

		public int SampleCount
		{
			get
			{
				if (Count == 0) return 0;
				return Sync.Reading(() => this.Min(s => s.Result.Count));
			}
		}

		public bool HasConverged(uint minSamples = 100, double convergence = 1, double tolerance = 0)
		{
			if (minSamples > SampleCount) return false;
			var scores = Sync.Reading(() => this.Select(v => v.Result.Average).ToArray());
			foreach (var s in scores)
			{
				if (s > convergence + double.Epsilon)
					throw new Exception("Score has exceeded convergence value: " + s);
				if (s < convergence - tolerance)
					return false;
			}
			return true;
		}


		public double[] Scores
		{
			get
			{
				return Sync.Reading(() => this.Select(v => v.Result.Average).ToArray());
			}
		}


		public void AddTheseScores(IEnumerable<double> scores)
		{
			Sync.Modifying(() =>
			{
				var i = 0;
				var count = Count;
				foreach (var n in scores)
				{
					SingleFitness f;
					if (i < count)
					{
						f = this[i];
					}
					else
					{
						this.Add(f = new SingleFitness());
					}

					f.Add(n);
					i++;
				}
			});

		}

		public void AddScores(params double[] scores)
		{
			this.AddTheseScores(scores);
		}

		static long FitnessCount = 0;
		long _id = Interlocked.Increment(ref FitnessCount);

		// Some cases enumerables are easier to sort in ascending than descnding.
		const int DIRECTION = -1;
		public int CompareTo(Fitness other)
		{
			if (this == other) return 0;
			if (other == null)
				throw new ArgumentNullException("other");
			int len = Count, otherLen = other.Count;
			if (len != 0 || otherLen != 0)
			{
				// If unseen? Should be of greater importance..
				if (len == 0 && otherLen != 0) return +DIRECTION;
				if (len != 0 && otherLen == 0) return -DIRECTION;
				Debug.Assert(len == other.Count, "Fitnesses must be compatible.");

				// In non-debug, all for the lesser scored to be of lesser importance.
				if (len < otherLen) return -DIRECTION;
				if (len > otherLen) return +DIRECTION;

				for (var i = 0; i < len; i++)
				{
					var a = this[i].Result;
					var b = other[i].Result;
					var aA = a.Average;
					var bA = b.Average;

					// Standard A less than B.
					if (aA < bA || double.IsNaN(aA) && !double.IsNaN(bA)) return -DIRECTION;
					// Standard A greater than B.
					if (aA > bA || !double.IsNaN(aA) && double.IsNaN(bA)) return +DIRECTION;

					// Who has the most samples?
					if (a.Count < b.Count) return -DIRECTION;
					if (a.Count > b.Count) return +DIRECTION;
				}
			}

			if (_id < other._id) return +DIRECTION;
			if (_id > other._id) return -DIRECTION;

			throw new Exception("Impossible? Interlocked failed?");
		}


	}
}