using System;

namespace Project2
{
	public delegate bool MemoryStrategy(
		int headSegment,
		int[] mainMemory,
		int minSegmentSize,
		out int segmentAddress,
		out int segmentsInspected);

	public static class MemoryStrategies
	{
		public static MemoryStrategy FirstFit()
		{
			return (int headSegment, int[] mainMemory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
			{
				segmentsInspected = 0;
				segmentAddress = headSegment;
				while(segmentAddress != MemoryManager.NullPointer)
				{
					segmentsInspected++;
					if (-mainMemory[segmentAddress] >= minSegmentSize)
						return true;
					segmentAddress = mainMemory[MemoryManager.NextPtrAddr(segmentAddress)];
				}
				return false;
			};
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