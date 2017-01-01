
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
				Debug.Assert(!double.IsNaN(value), "Adding a NaN value will completely invalidate the fitness value.");
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

	public interface IFitness : IComparable<IFitness>
	{
		int SampleCount { get; }
		IReadOnlyList<double> Scores { get; }

		int Count { get; }

		long ID { get; }

		ProcedureResult GetResult(int index);
	}

    public struct FitnessScore : IFitness
    {
		readonly List<ProcedureResult> _results;
		
		public FitnessScore(IFitness source)
		{
			var len = source.Count;
			Count = len;
			ID = source.ID;
			SampleCount = source.SampleCount;
			Scores = source.Scores;
			_results = new List<ProcedureResult>();
			for(var i = 0;i<len;i++)
				_results.Add(source.GetResult(i));
		}

        public int Count
        {
            get;
			private set;
        }

        public long ID
        {
            get;
			private set;
        }

        public int SampleCount
        {
            get;
			private set;
        }

        public IReadOnlyList<double> Scores
        {
            get;
			private set;
        }

        public int CompareTo(IFitness other)
        {
			return Fitness.Comparison(this, other);
        }

        public ProcedureResult GetResult(int index)
        {
			return _results[index];
        }
    }

    public class Fitness : ThreadSafeTrackedList<SingleFitness>, IFitness
	{

		public Fitness()
		{
			ID = Interlocked.Increment(ref FitnessCount);
		}

		public int SampleCount
		{
			get
			{
				if (Count == 0) return 0;
				return Sync.Reading(() => this.Min(s => s.Result.Count));
			}
		}

		public ProcedureResult GetResult(int index)
		{
			return this[index].Result;
		}

		public IReadOnlyList<double> Scores
		{
			get
			{
				return Sync.Reading(() => this.Select(v => v.Result.Average).ToList()).AsReadOnly();
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
		public long ID
		{
			get;
			private set;
		}

		internal int TestingCount = 0;

		// Some cases enumerables are easier to sort in ascending than descnding.
		const int DIRECTION = -1;
		public int CompareTo(IFitness other)
		{
			return Comparison(this, other);
		}

		public class Comparer : IComparer<IFitness>
		{
			public int Compare(IFitness x, IFitness y)
			{
				return Comparison(x, y);
			}
		}

		public static int Comparison(IFitness x, IFitness y)
		{

			if (x == y) return 0;
			if (y == null)
				throw new ArgumentNullException("other");
			int xLen = x.Count, yLen = y.Count;
			if (xLen != 0 || yLen != 0)
			{
				// If unseen? Should be of greater importance..
				if (xLen == 0 && yLen != 0) return +DIRECTION;
				if (xLen != 0 && yLen == 0) return -DIRECTION;
				Debug.Assert(xLen == y.Count, "Fitnesses must be compatible.");

				// In non-debug, all for the lesser scored to be of lesser importance.
				if (xLen < yLen) return -DIRECTION;
				if (xLen > yLen) return +DIRECTION;

				for (var i = 0; i < xLen; i++)
				{
					var a = x.GetResult(i);
					var b = y.GetResult(i);
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

			if (x.ID < y.ID) return +DIRECTION;
			if (x.ID > y.ID) return -DIRECTION;

			throw new Exception("Impossible? Interlocked failed?");

		}

	}

	public static class FitnessExtensions
	{
		public static bool HasConverged(this IFitness fitness, uint minSamples = 100, double convergence = 1, double tolerance = 0)
		{
			if (minSamples > fitness.SampleCount) return false;
			foreach (var s in fitness.Scores)
			{
				if (s > convergence + double.Epsilon)
					throw new Exception("Score has exceeded convergence value: " + s);
				if (s < convergence - tolerance)
					return false;
			}
			return true;
		}

		public static FitnessScore SnapShot(this IFitness fitness)
		{
			return new FitnessScore(fitness);
		}
	}
}