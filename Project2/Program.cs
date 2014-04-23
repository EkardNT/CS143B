using System;
using System.Collections.Generic;
namespace Project2
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			var memory = new MemoryManager(200, MemoryStrategies.FirstFit());
			int allocation, holesExamined;
			for (int i = 0; i < 10; i++)
			{
				memory.Request(10, out allocation, out holesExamined);
			}
			memory.Request(20, out allocation, out holesExamined);
			Console.Read();
		}

		private static void Driver(
			double averageRequestSize,
			double standardDeviation,
			Random random,
			int simulationSteps,
			int memorySize,
			MemoryStrategy strategy)
		{
			// Setup driver.
			var mm = new MemoryManager(memorySize, strategy);
			var reserved = new List<int>();

			// Statistics
			int totalHolesExamined = 0;

			for (int i = 0; i < simulationSteps; i++)
			{
				int requestSize = random.NextGaussian(averageRequestSize, standardDeviation, 1, memorySize);
				int allocation, holesExamined;
				while (mm.Request(requestSize, out allocation, out holesExamined))
				{
					reserved.Add(allocation);
					int placeToSwap = random.Next(reserved.Count);
					reserved[reserved.Count - 1] = reserved[placeToSwap];
					reserved[placeToSwap] = allocation;

					totalHolesExamined += holesExamined;

					requestSize = random.NextGaussian(averageRequestSize, standardDeviation, 1, memorySize);
				}
				totalHolesExamined += holesExamined;
			}
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