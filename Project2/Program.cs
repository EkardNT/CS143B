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
			foreach(int memorySize in MemorySizes)
			{
				var rand = new Random();
				using(var writer = File.CreateText(string.Format("memory{0}.csv", memorySize)))
				{
					writer.WriteLine("a,d,Average Memory Utilization,Average Search Time");
					for (int a = 100; a <= memorySize / 3; a += memorySize / 100)
					{
						for (int d = 50; d <= memorySize / 3; d += memorySize / 100)
						{
							int aCopy = a,
								dCopy = d;
							double averageMemoryUtilization, averageSearchTime;
							Driver(
								aCopy,
								dCopy,
								new Random(rand.Next()),
								SimulationSteps,
								memorySize,
								MemoryStrategies.FirstFit,
								out averageSearchTime,
								out averageMemoryUtilization);
							writer.WriteLine("{0},{1},{2:F4},{3:F4}", a, d, averageMemoryUtilization, averageSearchTime);
						}
					}
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