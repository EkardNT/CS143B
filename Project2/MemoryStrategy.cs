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
		public static MemoryStrategy FirstFit
		{
			get
			{
				return (int headSegment, int[] mainMemory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					int _segmentAddr = MemoryManager.NullPointer;
					segmentsInspected = MemoryManager.TraverseFreeList(mainMemory, headSegment, segment =>
					{
						if (-mainMemory[segment] >= minSegmentSize)
						{
							_segmentAddr = segment;
							return false;
						}
						return true;
					});
					return (segmentAddress = _segmentAddr) != MemoryManager.NullPointer;
				};
			}
		}

		public static MemoryStrategy NextFit
		{
			get
			{
				int nextStartAddr = MemoryManager.NullPointer;
				return (int headSegment, int[] mainMemory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					if (nextStartAddr == MemoryManager.NullPointer)
						nextStartAddr = headSegment;
					else if (mainMemory[nextStartAddr] == 0)
						nextStartAddr = headSegment;
					int _segmentAddr = MemoryManager.NullPointer;
					segmentsInspected = MemoryManager.TraverseFreeList(mainMemory, nextStartAddr, segment =>
					{
						if (-mainMemory[segment] >= minSegmentSize)
						{
							nextStartAddr = mainMemory[MemoryManager.NextPtrAddr(segment)];
							if (nextStartAddr == segment)
								nextStartAddr = MemoryManager.NullPointer;
							_segmentAddr = segment;
							return false;
						}
						return true;
					});
					return (segmentAddress = _segmentAddr) != MemoryManager.NullPointer;
				};
			}
		}

		public static MemoryStrategy BestFit
		{
			get
			{
				return (int headSegment, int[] mainMemory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					int _segmentAddr = MemoryManager.NullPointer,
						minSize = int.MaxValue;
					segmentsInspected = MemoryManager.TraverseFreeList(mainMemory, headSegment, segment =>
					{
						int segmentSize = -mainMemory[segment];
						if (segmentSize == minSegmentSize)
						{
							_segmentAddr = segment;
							return false;
						}
						if (segmentSize >= minSegmentSize && segmentSize < minSize)
						{
							minSize = segmentSize;
							_segmentAddr = segment;
						}
						return true;
					});
					return (segmentAddress = _segmentAddr) != MemoryManager.NullPointer;
				};
			}
		}

		public static MemoryStrategy WorstFit
		{
			get
			{
				return (int headSegment, int[] mainMemory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					int _segmentAddr = MemoryManager.NullPointer,
						maxSize = int.MinValue;
					segmentsInspected = MemoryManager.TraverseFreeList(mainMemory, headSegment, segment =>
					{
						int segmentSize = -mainMemory[segment];
						if (segmentSize == minSegmentSize)
						{
							_segmentAddr = segment;
							return false;
						}
						if (segmentSize >= minSegmentSize && segmentSize > maxSize)
						{
							maxSize = segmentSize;
							_segmentAddr = segment;
						}
						return true;
					});
					return (segmentAddress = _segmentAddr) != MemoryManager.NullPointer;
				};
			}
		}
	}
}