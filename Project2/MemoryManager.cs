using System;

namespace Project2
{
	public class MemoryManager
	{
		// Number of words of bookkeeping overhead required within a free segment.
		private const int OverheadPerFreeSegment = 4;
		// Number of words of bookkeeping overhead required within a reserved segment.
		private const int OverheadPerReservedSegment = 2;
		// Offset in a reserved segment from the segment start address to the first
		// user-usable word.
		private const int OffsetToFirstUsableWord = 1;
		// Value of previous and/or next segment pointers when they do
		// not point to any segment.
		public const int NullPointer = -1;

		private int headFreeSegment;
		private readonly int[] mainMemory;
		private readonly MemoryStrategy strategy;

		public MemoryManager(int memorySize, MemoryStrategy strategy)
		{
			if (memorySize < OverheadPerFreeSegment)
				throw new ArgumentOutOfRangeException("memorySize",
					string.Format("memorySize must be at least {0}, the amount of overhead per hole.", OverheadPerFreeSegment));
			if (strategy == null)
				throw new ArgumentNullException("strategy");

			this.strategy = strategy;

			// Initialize memory to single free segment.
			mainMemory = new int[memorySize];
			headFreeSegment = 0;
			// Tags, where the negative means this is a free segment.
			mainMemory[EndTagAddr(headFreeSegment)] = mainMemory[headFreeSegment] = -memorySize;
			// Prev and Next pointers.
			mainMemory[PrevPtrAddr(headFreeSegment)] = mainMemory[NextPtrAddr(headFreeSegment)] = NullPointer;
		}

		public bool Request(int count, out int allocation, out int holesExamined)
		{
			allocation = NullPointer;
			holesExamined = 0;

			if (count < 1 || count > mainMemory.Length)
				throw new ArgumentOutOfRangeException("count");

			// Ask the strategy to find the segment to allocate.
			// If it cannot, the request fails.
			int reservedSize = count + OverheadPerReservedSegment,
				segmentAddr;
			if (!strategy(headFreeSegment, mainMemory, reservedSize, out segmentAddr, out holesExamined))
				return false;

			// Make sure the strategy didn't give us an invalid segment.
			int segmentSize = Math.Abs(mainMemory[allocation]);
			if (reservedSize > segmentSize)
				throw new InvalidOperationException("Memory strategy returned a segment of insufficient size.");
			if (mainMemory[allocation] > 0)
				throw new InvalidOperationException("Memory strategy returned an already reserved segment.");

			// Split if there is space for a newly split segment with at least
			// one user-usable element.
			int splitSize = segmentSize - reservedSize;
			if (splitSize >= OverheadPerFreeSegment)
			{
				int splitSegmentAddr = segmentAddr + count + OverheadPerReservedSegment;
				mainMemory[EndTagAddr(splitSegmentAddr)] = mainMemory[splitSegmentAddr] = -splitSize;
				mainMemory[PrevPtrAddr(splitSegmentAddr)] = segmentAddr;
				mainMemory[NextPtrAddr(splitSegmentAddr)] = mainMemory[NextPtrAddr(segmentAddr)];
				mainMemory[NextPtrAddr(segmentAddr)] = splitSegmentAddr;
				if (mainMemory[NextPtrAddr(splitSegmentAddr)] != NullPointer)
					mainMemory[PrevPtrAddr(mainMemory[NextPtrAddr(splitSegmentAddr)])] = splitSegmentAddr;
			}

			// Adjust size.
			mainMemory[EndTagAddr(segmentAddr)] = mainMemory[segmentAddr] = reservedSize;

			// Remove the segment from the linked list of free segments.
			if (mainMemory[NextPtrAddr(segmentAddr)] != NullPointer)
			{
				if (headFreeSegment == segmentAddr)
					headFreeSegment = mainMemory[NextPtrAddr(segmentAddr)];
				mainMemory[PrevPtrAddr(mainMemory[NextPtrAddr(segmentAddr)])] = mainMemory[PrevPtrAddr(segmentAddr)];
				mainMemory[NextPtrAddr(segmentAddr)] = NullPointer;
			}
			if (mainMemory[PrevPtrAddr(segmentAddr)] != NullPointer)
			{
				if (headFreeSegment == segmentAddr)
					headFreeSegment = mainMemory[PrevPtrAddr(segmentAddr)];
				mainMemory[NextPtrAddr(mainMemory[PrevPtrAddr(segmentAddr)])] = mainMemory[NextPtrAddr(segmentAddr)];
				mainMemory[PrevPtrAddr(segmentAddr)] = NullPointer;
			}

			// If this segment was the head segment and both of the above
			// if blocks were skipped, then this was also the last free
			// segment.
			if (headFreeSegment == segmentAddr)
				headFreeSegment = NullPointer;

			allocation = segmentAddr + OffsetToFirstUsableWord;
			return true;
		}

		public void Release(int allocation)
		{
			if (mainMemory[allocation] < 0)
				throw new ArgumentException("Attempted to release non-reserved memory.", "allocation");

			int
				segmentAddr = allocation - OffsetToFirstUsableWord,
				leftNeighborAddr,
				rightNeighborAddr;
			bool
				leftNeighborFree = LeftNeighborAddr(segmentAddr, out leftNeighborAddr) && mainMemory[leftNeighborAddr] < 0,
				rightNeighborFree = RightNeighborAddr(segmentAddr, out rightNeighborAddr) && mainMemory[rightNeighborAddr] < 0;

			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			if (!leftNeighborFree && !rightNeighborFree)
			{
				// Case 0: If neither are free, add this segment to
				// the head of the linked list.
				mainMemory[EndTagAddr(segmentAddr)] = mainMemory[segmentAddr] = -mainMemory[segmentAddr];
				mainMemory[NextPtrAddr(segmentAddr)] = headFreeSegment; // Possibly NullPointer.
				mainMemory[PrevPtrAddr(segmentAddr)] = NullPointer;
				if (headFreeSegment != NullPointer)
					mainMemory[PrevPtrAddr(headFreeSegment)] = segmentAddr;
				headFreeSegment = segmentAddr;
			}
			else if (!leftNeighborFree && rightNeighborFree)
			{
				// Case 1: If only right neighbor is free, coalesce
				// the right neighbor into this segment, preserving
				// the right segment's linked list pointers.
			}
			else if (leftNeighborFree && !rightNeighborFree)
			{
				// Case 2: If only left neighbor is free, coalesce
				// this segment into the left neighbor, preserving the
				// left segment's linked list pointers.
			}
			else if (leftNeighborFree && rightNeighborFree)
			{
				// Case 3: If both neighbors are free, coalesce this
				// segment with both neighbors, handling the linked
				// list pointers appropriately.
			}
			// ReSharper restore ConditionIsAlwaysTrueOrFalse
		}

		private int EndTagAddr(int segmentAddr)
		{
			return segmentAddr + Math.Abs(mainMemory[segmentAddr]) - 1;
		}

		private int NextPtrAddr(int segmentAddr)
		{
			return segmentAddr + 2;
		}

		private int PrevPtrAddr(int segmentAddr)
		{
			return segmentAddr + 1;
		}

		private bool LeftNeighborAddr(int segmentAddr, out int leftNeighborAddr)
		{
			leftNeighborAddr = segmentAddr - Math.Abs(mainMemory[segmentAddr - 1]);
			return leftNeighborAddr >= 0;
		}

		private bool RightNeighborAddr(int segmentAddr, out int rightNeighborAddr)
		{
			rightNeighborAddr = segmentAddr + Math.Abs(mainMemory[segmentAddr]);
			return rightNeighborAddr < mainMemory.Length;
		}
	}
}