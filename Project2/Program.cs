using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project2
{
	internal static class Program
	{
		private class Allocation
		{
			public int Address { get; private set; }
			public int Size { get; private set; }

			public Allocation(int address, int size)
			{
				Address = address;
				Size = size;
			}
		}

		private class Case
		{
			public int MemorySize;
			public int AMin, AMax, AStep;
			public int DMin, DMax, DStep;
		}
		
		private static Case[] Cases =
		{
			new Case
			{
				MemorySize = 1000,
				AMin = 10,
				AMax = 300,
				AStep = 29,
				DMin = 10,
				DMax = 200,
				DStep = 19
			},
			new Case
			{
				MemorySize = 10000,
				AMin = 100,
				AMax = 3000,
				AStep = 290,
				DMin = 100,
				DMax = 2000,
				DStep = 190
			},
			new Case
			{
				MemorySize = 100000,
				AMin = 1000,
				AMax = 30000,
				AStep = 2900,
				DMin = 1000,
				DMax = 20000,
				DStep = 1900
			},
			new Case
			{
				MemorySize = 1000000,
				AMin = 10000,
				AMax = 300000,
				AStep = 29000,
				DMin = 10000,
				DMax = 200000,
				DStep = 19000
			},
		};
		private const int SimulationSteps = 10000;

		private static void Main()
		{
			//double avgS, avgU;
			//Driver(10, 5, new Random(973102217), SimulationSteps, 100, MemoryStrategies.NextFit, out avgS, out avgU);
			//Console.WriteLine("FINISHED");
			//Console.Read();
			//return;
			var tasks = new List<Task>();

			foreach(var @case in Cases)
			{
				var rand = new Random();
				using (var nextFitWriter = File.CreateText(string.Format("next_memory{0}.csv", @case.MemorySize)))
				using (var worstFitWriter = File.CreateText(string.Format("worst_memory{0}.csv", @case.MemorySize)))
				{
					nextFitWriter.WriteLine("Request Size Average,Request Size Std Dev,Average Memory Utilization,Average Search Time");
					worstFitWriter.WriteLine("Request Size Average,Request Size Std Dev,Average Memory Utilization,Average Search Time");
					for (int a = @case.AMin; a <= @case.AMax; a += @case.AStep)
					{
						for (int d = @case.DMin; d <= @case.DMax; d += @case.DStep)
						{
							var caseCopy = @case;
							int aCopy = a,
								dCopy = d;
							double averageMemoryUtilization, averageSearchTime;
							int seed = rand.Next();
							tasks.Add(Task.Run(() =>
							{
								Driver(
									aCopy,
									dCopy,
									new Random(seed),
									SimulationSteps,
									caseCopy.MemorySize,
									MemoryStrategies.NextFit,
									out averageSearchTime,
									out averageMemoryUtilization);
								nextFitWriter.WriteLine("{0},{1},{2:F4},{3:F4}", aCopy, dCopy, averageMemoryUtilization, averageSearchTime);
								Console.WriteLine("Next fit: {0},{1},{2:F4},{3:F4},{4}", aCopy, dCopy, averageMemoryUtilization, averageSearchTime, caseCopy.MemorySize);
							}));
							tasks.Add(Task.Run(() =>
							{
								Driver(
									aCopy,
									dCopy,
									new Random(seed),
									SimulationSteps,
									caseCopy.MemorySize,
									MemoryStrategies.WorstFit,
									out averageSearchTime,
									out averageMemoryUtilization);
								worstFitWriter.WriteLine("{0},{1},{2:F4},{3:F4}", aCopy, dCopy, averageMemoryUtilization, averageSearchTime);
								Console.WriteLine("Worst fit: {0},{1},{2:F4},{3:F4},{4}", aCopy, dCopy, averageMemoryUtilization, averageSearchTime, caseCopy.MemorySize);
							}));
						}
					}
					Task.WaitAll(tasks.ToArray());
					tasks.Clear();
					Console.WriteLine("Woot!");
				}				
			}
		}

		private static void Driver(
			double averageRequestSize,
			double standardDeviation,
			Random random,
			int simulationSteps,
			int memorySize,
			MemoryStrategy strategy,
			out double averageSearchTime,
			out double averageMemoryUtilization)
		{
			// Setup driver.
			var mm = new MemoryManager(memorySize, strategy);
			var reserved = new List<Allocation>();

			// Statistics
			long totalHolesExamined = 0;
			double totalMemoryUtilization = 0;

			for (int i = 0; i < simulationSteps; i++)
			{
				int requestSize = random.NextGaussian(averageRequestSize, standardDeviation, 1, memorySize);
				int allocationAddr, holesExamined;
				Allocation alloc;
				while (mm.Request(requestSize, out allocationAddr, out holesExamined))
				{
					reserved.Add(alloc = new Allocation(allocationAddr, requestSize));
					int placeToSwap = random.Next(reserved.Count);
					reserved[reserved.Count - 1] = reserved[placeToSwap];
					reserved[placeToSwap] = alloc;
					totalHolesExamined += holesExamined;
					requestSize = random.NextGaussian(averageRequestSize, standardDeviation, 1, memorySize);
				}
				// Count holes examined by failed request.
				totalHolesExamined += holesExamined;
				// Record memory utilization.
				totalMemoryUtilization += reserved.Sum(allocation => allocation.Size) / (double)memorySize;
				// Release a random reserved segment. Because the reserved list
				// is randomly ordered, we simply (and efficiently) remove the
				// last element.
				if(reserved.Count > 0)
				{
					mm.Release(reserved[reserved.Count - 1].Address);
					reserved.RemoveAt(reserved.Count - 1);
				}
			}

			averageSearchTime = totalHolesExamined / (double) simulationSteps;
			averageMemoryUtilization = totalMemoryUtilization / simulationSteps;
		}

		private static int NextGaussian(this Random random, double mean, double stDev, int min, int max)
		{
			int gaussian;
			do
			{
				double u1 = random.NextDouble(),
				u2 = random.NextDouble();
				while (u1 == 0)
					u1 = random.NextDouble();
				while (u2 == 0)
					u2 = random.NextDouble();
				gaussian = (int)Math.Round(stDev * Math.Sqrt(-2.0 * Math.Log(u1, Math.E)) * Math.Cos(2.0 * Math.PI * u2) + mean);
			} while (gaussian < min || gaussian > max);
			return gaussian;
		}
	}
}