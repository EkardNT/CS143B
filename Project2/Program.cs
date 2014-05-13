using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

		private const int SimulationSteps = 10000;
		private static int[] MemorySizes = new[]
		{
			1000, 10000, 100000, 1000000
		};

		private static void Main()
		{
			var tasks = new List<Task>();
			StringBuilder 
				nextFitBuilder = new StringBuilder(),
				worstFitBuilder = new StringBuilder();

			foreach(var memorySize in MemorySizes)
			{
				var rand = new Random();
				// Request size average in the range [0.1%, 20%] of main memory size.
				for (int a = 1; a <= 200; a++)
				{
					// Request size standard deviation in the range [0.1%, 10%] of main memory size.
					for (int d = 1; d <= 100; d++)
					{
						int aCopy = a,
							dCopy = d;
						double
							requestSizeAverage = a / 10.0,
							requestSizeStandardDeviation = d / 10.0;
						double averageMemoryUtilization, averageSearchTime;
						int seed = rand.Next();
						tasks.Add(Task.Run(() =>
						{
							Driver(
								requestSizeAverage,
								requestSizeStandardDeviation,
								new Random(seed),
								SimulationSteps,
								memorySize,
								AllocationStrategies.NextFit,
								out averageSearchTime,
								out averageMemoryUtilization);
							lock (nextFitBuilder)
							{
								nextFitBuilder.AppendLine(string.Format("{0},{1},{2:F4},{3:F4}", requestSizeAverage, requestSizeStandardDeviation, averageMemoryUtilization, averageSearchTime));
							}
						}));
						tasks.Add(Task.Run(() =>
						{
							Driver(
								requestSizeAverage,
								requestSizeStandardDeviation,
								new Random(seed),
								SimulationSteps,
								memorySize,
								AllocationStrategies.WorstFit,
								out averageSearchTime,
								out averageMemoryUtilization);
							lock(worstFitBuilder)
							{
								worstFitBuilder.AppendLine(string.Format("{0},{1},{2:F4},{3:F4}", requestSizeAverage, requestSizeStandardDeviation, averageMemoryUtilization, averageSearchTime));
							}
							if (dCopy % 20 == 0)
								Console.WriteLine("a: {0}, d: {1}", aCopy, dCopy);
						}));
					}
				}

				Task.WaitAll(tasks.ToArray());
				tasks.Clear();

				tasks.Add(Task.Run(() =>
				{
					using (var nextFitWriter = File.CreateText(string.Format("next_memory{0}.csv", memorySize)))
					{
						nextFitWriter.WriteLine("Request Size Average,Request Size Std Dev,Average Memory Utilization,Average Search Time");
						nextFitWriter.WriteLine(nextFitBuilder.ToString());
					}
				}));
				tasks.Add(Task.Run(() =>
				{
					using (var worstFitWriter = File.CreateText(string.Format("worst_memory{0}.csv", memorySize)))
					{
						worstFitWriter.WriteLine("Request Size Average,Request Size Std Dev,Average Memory Utilization,Average Search Time");
						worstFitWriter.WriteLine(worstFitBuilder.ToString());
					}
				}));

				Task.WaitAll(tasks.ToArray());
				tasks.Clear();

				nextFitBuilder.Clear();
				worstFitBuilder.Clear();

				Console.WriteLine("Woot!");
			}
		}

		private static void Driver(
			double averageRequestSize,
			double standardDeviation,
			Random random,
			int simulationSteps,
			int memorySize,
			AllocationStrategy strategy,
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