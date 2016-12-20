using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	/// <summary>
	/// This is the container class for a genome.  It tracks the progress of a specific Genome.
	/// </summary>
	class Organism
	{

		ConcurrentBag<double> _scores;

		public Organism(Genome genome)
		{
			_scores = new ConcurrentBag<double>();

			Genome = genome;
		}

		public Genome Genome
		{
			get;
			private set;
		}

		public IReadOnlyCollection<double> FitnessScores
		{
			get
			{
				return _scores.ToList().AsReadOnly();
			}
		}

		public double FitnessAverage
		{
			get
			{
				return _scores.Any() ? _scores.Average() : double.NaN;
			}
		}

		public int Cycles
		{
			get
			{
				return _scores.Count;
			}
		}

		public void AddFitness(double result)
		{
			_scores.Add(result);
		}


	}
}
