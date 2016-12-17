/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithmPlatform
{
    public class Population<TGenome> : IPopulation<TGenome>
    where TGenome : IGenome

    {
        private ConcurrentDictionary<string, TGenome> _population;
        private IGenomeFactory<TGenome> _genomeFactory;

        public Population(IGenomeFactory<TGenome> genomeFactory)
        {
            _population = new ConcurrentDictionary<string, TGenome>();
            _genomeFactory = genomeFactory;
        }


        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        int Count
        {
            get
            {
                return _population.Count;
            }
        }

        int ICollection<TGenome>.Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Remove(TGenome item)
        {
            if (item != null)
            {
                TGenome r;
                return _population.TryRemove(item.Hash, out r);
            }
            return false;
        }

        public void Clear()
        {
            _population.Clear();
        }

        public bool Contains(TGenome item)
        {
            return item != null && _population.ContainsKey(item.Hash);
        }

        public void CopyTo(TGenome[] target, int index = 0)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (index < 0) throw new ArgumentOutOfRangeException("index");

            // This is a generic implementation that will work for all derived classes.
            // It can be overridden and optimized.
            var e = _population.GetEnumerator();
            while (e.MoveNext()) // Disposes when finished.
            {
                target[index++] = e.Current.Value;
            }
        }


        IEnumerator<TGenome> GetEnumeratorInternal()
        {
            return _population
                .Select(o => o.Value)
                .GetEnumerator();
        }

        public IEnumerator<TGenome> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        public void Add(TGenome potential)
        {
            string ts;
            if (potential != null && !_population.ContainsKey(ts = potential.Hash))
            {
                _population.TryAdd(ts, potential);
            }
        }
        public async void Add()
        {
            // Be sure to add randomness in...
            Add(await _genomeFactory.Generate());
        }

        public void Add(IEnumerable<TGenome> genomes)
        {
            if (genomes != null)
                foreach (var g in genomes)
                    Add(g);
        }


        public async Task Populate(uint count, TGenome[] rankedGenomes = null)
        {
            if (rankedGenomes != null && rankedGenomes.Length != 0)
            {
                var top = rankedGenomes[0];
                if (top.VariationCountdown == 0)
                {
                    top.VariationCountdown = 20;
                    var v = await _genomeFactory.GenerateVariations(top);
                    Console.WriteLine("Top Variations:", v.Length);
                    Add(v);
                }
                else
                {
                    top.VariationCountdown--;
                }
            }

            // Then add mutations from best in source.
            for (var i = 0; i < count; i++)
            {
                Add(await _genomeFactory.Generate(rankedGenomes));
            }
        }

        // Provide a mechanism for culling the herd without requiring IProblem to be imported.
        public void KeepOnly(IEnumerable<TGenome> selected)
        {
            var hashed = new HashSet<string>(selected.Select(o => o.Hash));
            foreach (var o in _population.ToArray())
            {
                TGenome g;
                var key = o.Key;
                if (!hashed.Contains(key))
                    _population.TryRemove(key, out g);
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorInternal();
        }

    }
}