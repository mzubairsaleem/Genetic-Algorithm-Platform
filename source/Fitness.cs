
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GeneticAlgorithmPlatform
{


    public class SingleFitness : IReadOnlyList<double>, IComparable
    {
        private List<double> _source;
        private Lazy<double> _average;

        public SingleFitness(IEnumerable<double> scores = null)
        {
            _source = new List<double>();
            Reset();
            if (scores != null)
                Add(scores);
        }

        public int Count
        {
            get
            {
                return _source.Count;
            }
        }

        internal void Reset(IEnumerable<double> scores = null)
        {
            lock (_source)
            {
                _average = new Lazy<double>(GetAverage, LazyThreadSafetyMode.ExecutionAndPublication);
            }
        }

        internal void Add(double score)
        {
            lock (_source)
            {
                if (_average.IsValueCreated)
                    throw new InvalidOperationException("Attempting to add a fitness value after the average score was rendered.");
                _source.Add(score);
            }
        }

        internal void Add(IEnumerable<double> scores)
        {
            lock (_source)
            {
                foreach (var s in scores)
                    Add(s);
            }
        }

        private double GetAverage()
        {
            lock (_source)
            {
                return _source.Average();
            }
        }

        public int CompareTo(object obj)
        {
            var other = (SingleFitness)obj;
            var a = this.Average;
            var b = other.Average;
            if (a < b || double.IsNaN(a) && !double.IsNaN(b)) return -1;
            if (a > b || !double.IsNaN(a) && double.IsNaN(b)) return +1;
            return 0;
        }

        public IEnumerator<double> GetEnumerator()
        {
            return ((IReadOnlyList<double>)_source).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IReadOnlyList<double>)_source).GetEnumerator();
        }

        public double Average
        {
            get
            {
                if (Count == 0) return double.NaN;
                return _average.Value;
            }
        }


        public double this[int index]
        {
            get
            {
                return ((IReadOnlyList<double>)_source)[index];
            }
        }
    }

    public class Fitness : IList<SingleFitness>, IComparable
    {

        List<SingleFitness> _source;

        public Fitness()
        {
            _source = new List<SingleFitness>();
        }

        int SampleCount
        {
            get
            {
                if (_source.Count == 0) return 0;
                return _source.Min(s => s.Count);
            }
        }

        public bool HasConverged(uint minSamples = 100, float convergence = 1, float tolerance = 0)
        {
            if (minSamples > SampleCount) return false;

            foreach (var s in _source)
            {
                var score = s.Average;
                if (score > convergence)
                    throw new Exception("Score has exceeded convergence value.");
                if (score < convergence - tolerance)
                    return false;
            }
            return true;
        }


        public IEnumerable<double> Scores
        {
            get
            {
                return _source.Select(s => s.Average);
            }
        }



        public int Count
        {
            get
            {
                return _source.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public SingleFitness this[int index]
        {
            get
            {
                return _source[index];
            }

            set
            {
                lock(_source) _source[index] = value;
            }
        }



        public int IndexOf(SingleFitness item)
        {
            return _source.IndexOf(item);
        }

        public void Insert(int index, SingleFitness item)
        {
            lock(_source) _source.Insert(index, item);
        }


        public void RemoveAt(int index)
        {
           lock(_source) _source.RemoveAt(index);
        }

        public void Add(SingleFitness item)
        {
            lock(_source) _source.Add(item);
        }

        public void Add(IEnumerable<double> sample)
        {

            lock(_source) _source.Add(new SingleFitness(sample));
        }

        public void AddScores(IEnumerable<double> scores)
        {
            lock(_source)
            {
                var i = 0;
                var count = _source.Count;
                foreach (var n in scores)
                {
                    SingleFitness f;
                    if (i >= count)
                        _source[i] = f = new SingleFitness();
                    else
                        f = this[i];
                    f.Add(n);
                    i++;
                }
            }

        }

        public void AddScores(params double[] scores)
        {
            this.AddScores(scores);
        }



        public void Clear()
        {
            lock(_source) _source.Clear();
        }

        public bool Contains(SingleFitness item)
        {
            return _source.Contains(item);
        }

        public void CopyTo(SingleFitness[] array, int arrayIndex)
        {
            _source.CopyTo(array, arrayIndex);
        }

        public bool Remove(SingleFitness item)
        {
            lock(_source) return  _source.Remove(item);
        }

        public IEnumerator<SingleFitness> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        int IComparable.CompareTo(object obj)
        {
            return this.CompareTo((Fitness)obj);
        }

        public int CompareTo(Fitness other)
        {
            var len = _source.Count;
            for (var i = 0; i < len; i++)
            {
                var a = this[i];
                var b = other[i];
                var aA = a.Average;
                var bA = b.Average;

                if (aA < bA || double.IsNaN(aA) && !double.IsNaN(bA)) return -1;
                if (aA > bA || !double.IsNaN(aA) && double.IsNaN(bA)) return +1;

                if (a.Count < b.Count) return -1;
                if (a.Count > b.Count) return +1;
            }
            return 0;
        }

    }
}