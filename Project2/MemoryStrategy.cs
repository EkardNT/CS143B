using System;

namespace Project2
{
	public delegate bool MemoryStrategy(
		int[] mainMemory,
		int minSegmentSize,
		out int segmentAddress,
		out int segmentsInspected);

	public static class MemoryStrategies
	{
		public static MemoryStrategy FirstFit()
		{
			throw new NotImplementedException();
		}

		public static MemoryStrategy NextFit()
		{
			throw new NotImplementedException();
		}

		public static MemoryStrategy BestFit()
		{
			throw new NotImplementedException();
		}

		public static MemoryStrategy WorstFit()
		{
			throw new NotImplementedException();
		}
	}
}