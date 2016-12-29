/*!
 * @author electricessence / https://github.com/electricessence/
 * Licensing: MIT https://github.com/electricessence/Genetic-Algorithm-Platform/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Open.Arithmetic;
using Open.Collections;

namespace GeneticAlgorithmPlatform
{

	public abstract class Environment<TGenome> : IEnvironment<TGenome>
		where TGenome : IGenome
	{

		protected uint _generations = 0;
		protected LinkedList<Population<TGenome>> _populations;
		protected List<IProblem<TGenome>> _problems;
		private IGenomeFactory<TGenome> _genomeFactory;

		public bool Converged
		{
			get;
			protected set;
		}

		public int PopulationSize = 50;
		public int MaxPopulations = 20;
		public int TestCount = 5;


		protected Environment(IGenomeFactory<TGenome> genomeFactory)
		{
			this._genomeFactory = genomeFactory;
			this._problems = new List<IProblem<TGenome>>();
			this._populations = new LinkedList<Population<TGenome>>();
		}

		IEnumerable<Task> GenerateTests(int count)
		{
			var problems = _problems.ToArray();
			var population = _populations.First;
			while (population != null)
			{
				foreach (var problem in problems)
					yield return problem.TestAsync(population.Value.Values, count);
				population = population.Next;
			}
		}

		public void Test(int count = 1)
		{
			var problems = _problems.ToArray();
			var population = _populations.First;
			while (population != null)
			{
				foreach (var problem in problems)
					problem.Test(population.Value.Values, count);
				population = population.Next;
			}
		}

		public Task TestAsync(int count)
		{
			return Task.WhenAll(GenerateTests(count));
		}

		public Task TestAsync()
		{
			return TestAsync(TestCount);
		}

		public uint Generations
		{
			get
			{
				return _generations;
			}
		}

		//noinspection JSUnusedGlobalSymbols
		public int Populations
		{
			get { return _populations.Count; }
		}

		private long _totalTime = 0;

		protected virtual void _onExecute()
		{
			long time;
			Stopwatch sw = Stopwatch.StartNew();

			var allGenes = _populations.SelectMany(g => g.Values).ToArray();
			// Get ranked population for each problem and merge it into a weaved enumeration.
			var previousP = _problems.Select(r => r.Rank(allGenes));

			Population<TGenome> p = Spawn(
				PopulationSize, (allGenes.Any() && previousP.Any()) ?
					Triangular.Disperse.Decreasing(
						previousP.Weave()
					) : null
			);

			var beforeCulling = p.Count;
			if (beforeCulling == 0) // Just in case.
				throw new Exception("Nothing spawned!!!");

			// {
			// 	// Retain genomes on the pareto...
			// 	var elite = this.getNewPopulation();
			// 	elite.importEntries(_problems.selectMany(r => r.pareto(allGenes)));
			// 	this._populations.add(elite);
			// }

			Console.WriteLine("Generation: {0}", ++_generations);
			Console.WriteLine("Populations: {0}", _populations.Count);
			time = sw.ElapsedMilliseconds;
			_totalTime += time;
			Console.WriteLine("Selection/Ranking (ms): {0}", time);
			sw = Stopwatch.StartNew();

			// Test();
			TestAsync().Wait();

			// Since we have 'variations' added into the pool, we don't want to eliminate any new material that may be useful.
			var additional = Math.Max(p.Count - PopulationSize, 0);

			p.KeepOnly(
				_problems
					.Select(r => r.Rank(p.Values))
					.Weave()
					.Take(PopulationSize / 2 + additional)
			);
			Console.Write("Population Size: {0}", p.Count);
			Console.WriteLine(" / {0}", beforeCulling);


			Console.WriteLine("Testing/Cleanup (ms): {0}", sw.ElapsedMilliseconds);
			time = sw.ElapsedMilliseconds;
			_totalTime += time;
			Console.Write("Time: {0} current / ", time);
			Console.Write("{0} total", _totalTime);
			Console.WriteLine(" ({0} average)", Math.Floor(((double)_totalTime) / _generations));
		}


		/**
         * Adds a new population to the environment.  Optionally pulling from the source provided.
         */
		public Population<TGenome> Spawn(int populationSize, IEnumerable<TGenome> source = null)
		{
			var p = new Population<TGenome>(_genomeFactory);

			p.Populate(populationSize, source);

			_populations.AddLast(p);
			StartTrimming();
			return p;
		}

		void StartTrimming()
		{
			_genomeFactory.TrimPreviousGenomes();
			TrimEarlyPopulations();
		}

		Lazy<Task> _trimEarlyPopulations;
		public Task TrimEarlyPopulations(int maxPopulations)
		{
			return LazyInitializer.EnsureInitialized(ref _trimEarlyPopulations,
				() => Lazy.New(
					() => Task.Run(() =>
					{
						while (_populations.Count > maxPopulations)
						{
							var p = _populations.First.Value;
							// Move top items to latest population.
							foreach (var problem in _problems)
							{
								var keep = problem.Rank(p.Values).FirstOrDefault();
								if (keep != null) _populations.Last.Value.Add(keep);
							}
							_populations.RemoveFirst();
						}
						Interlocked.Exchange(ref _trimEarlyPopulations, null);
					})
				)
			).Value;
		}

		public Task TrimEarlyPopulations()
		{
			return TrimEarlyPopulations(MaxPopulations);
		}


		// public async Task RunOnceAsync()
		// {
		// 	await this._onExecute().ConfigureAwait(false);
		// }

		public void RunOnce()
		{
			this._onExecute();//.Wait();
		}

		// public async Task RunUntilConvergedAsync(uint maxGenerations = uint.MaxValue)
		// {
		// 	while (!Converged && this._generations < maxGenerations)
		// 		await this._onExecute().ConfigureAwait(false);
		// }

		public void RunUntilConverged(uint maxGenerations = uint.MaxValue)
		{
			while (!Converged && this._generations < maxGenerations)
				this._onExecute();//.Wait();
		}

        public Task<Population<TGenome>> SpawnAsync(int populationSize, IEnumerable<TGenome> source = null)
        {
			return Task.FromResult(Spawn(populationSize,source));
        }
    }


}
