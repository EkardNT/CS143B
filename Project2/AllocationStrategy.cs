namespace Project2
{
	public delegate bool AllocationStrategy(
		MemoryManager memory,
		int minSegmentSize,
		out int segmentAddress,
		out int segmentsInspected);

	public static class AllocationStrategies
	{
		public static AllocationStrategy FirstFit
		{
			get
			{
				return (MemoryManager memory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					int _segmentAddr = MemoryManager.NullPointer;
					segmentsInspected = memory.TraverseFreeList(memory.FreeListHead, segment =>
					{
						if (-memory[segment] >= minSegmentSize)
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

		public static AllocationStrategy NextFit
		{
			get
			{
				int nextStartAddr = MemoryManager.NullPointer;
				return (MemoryManager memory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					if (nextStartAddr == MemoryManager.NullPointer)
						nextStartAddr = memory.FreeListHead;
					else if (memory[nextStartAddr] == 0)
						nextStartAddr = memory.FreeListHead;
					int _segmentAddr = MemoryManager.NullPointer;
					segmentsInspected = memory.TraverseFreeList(nextStartAddr, segment =>
					{
						if (-memory[segment] >= minSegmentSize)
						{
							nextStartAddr = memory[memory.NextPtrAddr(segment)];
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

		public static AllocationStrategy BestFit
		{
			get
			{
				return (MemoryManager memory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					int _segmentAddr = MemoryManager.NullPointer,
						minSize = int.MaxValue;
					segmentsInspected = memory.TraverseFreeList(memory.FreeListHead, segment =>
					{
						int segmentSize = -memory[segment];
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

		public static AllocationStrategy WorstFit
		{
			get
			{
				return (MemoryManager memory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					int _segmentAddr = MemoryManager.NullPointer,
						maxSize = int.MinValue;
					segmentsInspected = memory.TraverseFreeList(memory.FreeListHead, segment =>
					{
						int segmentSize = -memory[segment];
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