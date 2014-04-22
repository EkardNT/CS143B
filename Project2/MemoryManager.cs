using System;

namespace Project2
{
	public class MemoryManager
	{
		// Number of words of bookkeeping overhead required for each hole.
		private const int OverheadPerSegment = 4;
		// Value of previous and/or next segment pointers when they do
		// not point to any segment.
		public const int NullPointer = -1;

		// Linear main memory array.
		private readonly int[] mainMemory;
		private readonly MemoryStrategy strategy;

		public MemoryManager(int memorySize, MemoryStrategy strategy)
		{
			if (memorySize < OverheadPerSegment)
				throw new ArgumentOutOfRangeException("memorySize",
					string.Format("memorySize must be at least {0}, the amount of overhead per hole.", OverheadPerSegment));
			if (strategy == null)
				throw new ArgumentNullException("strategy");

			this.strategy = strategy;

			// Initialize memory to single free segment.
			mainMemory = new int[memorySize];
			// Tags, where the negative means this is a free segment.
			mainMemory[EndTagAddr(0)] = mainMemory[0] = -memorySize;
			// Prev and Next pointers.
			mainMemory[PrevPtrAddr(0)] = mainMemory[NextPtrAddr(0)] = NullPointer;
		}

		public bool Request(int count, out int allocation, out int holesExamined)
		{
			allocation = NullPointer;
			holesExamined = 0;

			if (count <= 0 || count > mainMemory.Length)
				throw new ArgumentOutOfRangeException("count");

			// Ask the strategy to find the segment to allocate.
			// If it cannot, the request fails.
			int segmentAddress;
			if (!strategy(mainMemory, count + OverheadPerSegment, out segmentAddress, out holesExamined))
				return false;

			// Make sure the strategy didn't give us an invalid segment.
			int segmentSize = Math.Abs(mainMemory[allocation]);
			if (segmentSize - OverheadPerSegment < count)
				throw new InvalidOperationException("Memory strategy returned a segment of insufficient size.");
			if (mainMemory[allocation] > 0)
				throw new InvalidOperationException("Memory strategy returned an already reserved segment.");

			// Split if there is space for a newly split segment with at least
			// one user-usable element.
			int splitSize = segmentSize - (count + OverheadPerSegment);
			if (splitSize > OverheadPerSegment)
			{
				int splitSegmentAddress = segmentAddress + count + OverheadPerSegment;
				mainMemory[EndTagAddr(splitSegmentAddress)] = mainMemory[splitSegmentAddress] = -splitSize;
				mainMemory[PrevPtrAddr(splitSegmentAddress)] = segmentAddress;
				mainMemory[NextPtrAddr(segmentAddress)] = splitSegmentAddress;
				mainMemory[NextPtrAddr(splitSegmentAddress)] = mainMemory[NextPtrAddr(segmentAddress)];
				if (mainMemory[NextPtrAddr(splitSegmentAddress)] != NullPointer)
					mainMemory[PrevPtrAddr(mainMemory[NextPtrAddr(splitSegmentAddress)])] = splitSegmentAddress;
				mainMemory[EndTagAddr(segmentAddress)] = mainMemory[segmentAddress] = count + OverheadPerSegment;
			}

			// Remove the segment from the linked list of free segments.
			if (mainMemory[PrevPtrAddr(segmentAddress)] != NullPointer)
			{
				mainMemory[NextPtrAddr(mainMemory[PrevPtrAddr(segmentAddress)])] = mainMemory[NextPtrAddr(segmentAddress)];
				mainMemory[PrevPtrAddr(segmentAddress)] = NullPointer;
			}
			if (mainMemory[NextPtrAddr(segmentAddress)] != NullPointer)
			{
				mainMemory[PrevPtrAddr(mainMemory[NextPtrAddr(segmentAddress)])] = mainMemory[PrevPtrAddr(segmentAddress)];
				mainMemory[NextPtrAddr(segmentAddress)] = NullPointer;
			}

			allocation = segmentAddress + 3;
			return true;
		}

		public void Release(int allocation)
		{
			if (mainMemory[allocation] < 0)
				throw new ArgumentException("Attempted to release non-reserved memory.", "allocation");

			int segmentAddress = allocation - 3;

			// Mark the segment as freed.
		}

		private int EndTagAddr(int segmentStart)
		{
			return segmentStart + Math.Abs(mainMemory[segmentStart]) - 1;
		}

		private int NextPtrAddr(int segmentStart)
		{
			return segmentStart + 2;
		}

		private int PrevPtrAddr(int segmentStart)
		{
			return segmentStart + 1;
		}

		private bool LeftNeighborAddr(int segmentStart, out int leftNeighborAddr)
		{
			leftNeighborAddr = segmentStart - Math.Abs(mainMemory[segmentStart - 1]);
			return leftNeighborAddr >= 0;
		}

		private bool RightNeighborAddr(int segmentStart, out int rightNeighborAddr)
		{
			rightNeighborAddr = segmentStart + Math.Abs(mainMemory[segmentStart]);
			return rightNeighborAddr < mainMemory.Length;
		}
	}
}