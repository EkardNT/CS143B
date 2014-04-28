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
					segmentsInspected = 0;
					segmentAddress = headSegment;
					while (segmentAddress != MemoryManager.NullPointer)
					{
						segmentsInspected++;
						if (-mainMemory[segmentAddress] >= minSegmentSize)
							return true;
						segmentAddress = mainMemory[MemoryManager.NextPtrAddr(segmentAddress)];
					}
					return false;
				};
			}
		}

		public static MemoryStrategy NextFit
		{
			get
			{
				int currentSegmentAddr = MemoryManager.NullPointer;
				return (int headSegment, int[] mainMemory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					segmentsInspected = 0;
					segmentAddress = MemoryManager.NullPointer;

					if (headSegment == MemoryManager.NullPointer)
						return false;

					if (currentSegmentAddr == MemoryManager.NullPointer)
						currentSegmentAddr = headSegment;

					bool startedAtHead = currentSegmentAddr == headSegment;
					int startAddr = currentSegmentAddr;

					while (currentSegmentAddr != MemoryManager.NullPointer)
					{
						segmentsInspected++;
						if (-mainMemory[currentSegmentAddr] >= minSegmentSize)
						{
							segmentAddress = currentSegmentAddr;
							currentSegmentAddr = mainMemory[MemoryManager.NextPtrAddr(currentSegmentAddr)];
							return true;
						}
						currentSegmentAddr = mainMemory[MemoryManager.NextPtrAddr(currentSegmentAddr)];
					}
					// Loop back around.
					if (!startedAtHead)
					{
						currentSegmentAddr = headSegment;
						while (currentSegmentAddr != MemoryManager.NullPointer && currentSegmentAddr != startAddr)
						{
							segmentsInspected++;
							if (-mainMemory[currentSegmentAddr] >= minSegmentSize)
							{
								segmentAddress = currentSegmentAddr;
								currentSegmentAddr = mainMemory[MemoryManager.NextPtrAddr(currentSegmentAddr)];
								return true;
							}
							currentSegmentAddr = mainMemory[MemoryManager.NextPtrAddr(currentSegmentAddr)];
						}
					}
					return false;
				};
			}
		}

		public static MemoryStrategy BestFit
		{
			get
			{
				return (int headSegment, int[] mainMemory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					segmentsInspected = 0;
					segmentAddress = MemoryManager.NullPointer;
					int minSize = int.MaxValue,
						currentAddress = headSegment,
						segmentSize;
					while (currentAddress != MemoryManager.NullPointer)
					{
						segmentsInspected++;
						segmentSize = -mainMemory[currentAddress];
						if (segmentSize == minSegmentSize)
						{
							segmentAddress = currentAddress;
							return true;
						}
						if (segmentSize >= minSegmentSize && segmentSize < minSize)
						{
							minSize = segmentSize;
							segmentAddress = currentAddress;
						}
						currentAddress = mainMemory[MemoryManager.NextPtrAddr(currentAddress)];
					}
					return minSize != int.MaxValue;
				};
			}
		}

		public static MemoryStrategy WorstFit
		{
			get
			{
				return (int headSegment, int[] mainMemory, int minSegmentSize, out int segmentAddress, out int segmentsInspected) =>
				{
					segmentsInspected = 0;
					segmentAddress = MemoryManager.NullPointer;
					int maxSize = int.MinValue,
						currentAddress = headSegment,
						segmentSize;
					while (currentAddress != MemoryManager.NullPointer)
					{
						segmentsInspected++;
						segmentSize = -mainMemory[currentAddress];
						if (segmentSize == minSegmentSize)
						{
							segmentAddress = currentAddress;
							return true;
						}
						if (segmentSize >= minSegmentSize && segmentSize > maxSize)
						{
							maxSize = segmentSize;
							segmentAddress = currentAddress;
						}
						currentAddress = mainMemory[MemoryManager.NextPtrAddr(currentAddress)];
					}
					return maxSize != int.MinValue;
				};
			}
		}
	}
}