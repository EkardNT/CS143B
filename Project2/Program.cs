using System;
namespace Project2
{
	internal class Program
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
	}
}