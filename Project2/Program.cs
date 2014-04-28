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

		private static int[] MemorySizes = new[]
		{
			1000, 10000, 100000, 1000000
		};
		private const int SimulationSteps = 100000;

		private static void Main(string[] args)
		{
			double avgS, avgU;
			Driver(10, 5, new Random(973102217), SimulationSteps, 100, MemoryStrategies.NextFit, out avgS, out avgU);
			Console.WriteLine("FINISHED");
			Console.Read();
			return;
			var tasks = new Task[4];

			foreach(int memorySize in MemorySizes)
			{
				var rand = new Random();
				using(var firstFitWriter = File.CreateText(string.Format("first_memory{0}.csv", memorySize)))
				using(var nextFitWriter = File.CreateText(string.Format("next_memory{0}.csv", memorySize)))
				using(var bestFitWriter = File.CreateText(string.Format("best_memory{0}.csv", memorySize)))
				using(var worstFitWriter = File.CreateText(string.Format("worst_memory{0}.csv", memorySize)))
				{
					firstFitWriter.WriteLine("a,d,Average Memory Utilization,Average Search Time");
					nextFitWriter.WriteLine("a,d,Average Memory Utilization,Average Search Time");
					bestFitWriter.WriteLine("a,d,Average Memory Utilization,Average Search Time");
					worstFitWriter.WriteLine("a,d,Average Memory Utilization,Average Search Time");
					for (int a = 100; a <= memorySize / 3; a += memorySize / 100)
					{
						for (int d = 50; d <= memorySize / 3; d += memorySize / 100)
						{
							int aCopy = a,
								dCopy = d;
							double averageMemoryUtilization, averageSearchTime;
							int seed = rand.Next();
							Console.WriteLine("[Driver a:{0} b:{1} seed:{2} memorySize:{3}]",
								aCopy,
								dCopy,
								seed,
								memorySize);
							tasks[0] = Task.Run(() => {
								Driver(
									aCopy,
									dCopy,
									new Random(seed),
									SimulationSteps,
									memorySize,
									MemoryStrategies.FirstFit,
									out averageSearchTime,
									out averageMemoryUtilization);
								firstFitWriter.WriteLine("{0},{1},{2:F4},{3:F4}", a, d, averageMemoryUtilization, averageSearchTime);
							});
							tasks[1] = Task.Run(() => {
								Driver(
									aCopy,
									dCopy,
									new Random(seed),
									SimulationSteps,
									memorySize,
									MemoryStrategies.NextFit,
									out averageSearchTime,
									out averageMemoryUtilization);
								nextFitWriter.WriteLine("{0},{1},{2:F4},{3:F4}", a, d, averageMemoryUtilization, averageSearchTime);
							});
							tasks[2] = Task.Run(() => {
								Driver(
									aCopy,
									dCopy,
									new Random(seed),
									SimulationSteps,
									memorySize,
									MemoryStrategies.BestFit,
									out averageSearchTime,
									out averageMemoryUtilization);
								bestFitWriter.WriteLine("{0},{1},{2:F4},{3:F4}", a, d, averageMemoryUtilization, averageSearchTime);
							});
							tasks[3] = Task.Run(() => {
								Driver(
									aCopy,
									dCopy,
									new Random(seed),
									SimulationSteps,
									memorySize,
									MemoryStrategies.WorstFit,
									out averageSearchTime,
									out averageMemoryUtilization);
								worstFitWriter.WriteLine("{0},{1},{2:F4},{3:F4}", a, d, averageMemoryUtilization, averageSearchTime);
							});
							Task.WaitAll(tasks);
						}
					}
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