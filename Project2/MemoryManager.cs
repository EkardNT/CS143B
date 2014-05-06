using System;
using System.Diagnostics;

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
		private readonly AllocationStrategy strategy;

		public MemoryManager(int memorySize, AllocationStrategy strategy)
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
			mainMemory[headFreeSegment] = -memorySize;
			mainMemory[EndTagAddr(headFreeSegment)] = -memorySize;
			// Prev and Next pointers for a circular linked list.
			mainMemory[PrevPtrAddr(headFreeSegment)] = mainMemory[NextPtrAddr(headFreeSegment)] = 0;
		}

		public int FreeListHead { get { return headFreeSegment; } }

		public int this[int addr]
		{
			get { return mainMemory[addr]; }
		}

		public bool Request(int count, out int allocation, out int holesExamined)
		{
			allocation = NullPointer;
			holesExamined = 0;

			AssertStateCorrect();

			if (count < 1 || count > mainMemory.Length)
				throw new ArgumentOutOfRangeException("count");

			// Ask the strategy to find the segment to allocate.
			// If it cannot, the request fails. Note that the
			// min reservation size is the sum of the count and
			// the overhead per FREE segment, not reserved, because
			// otherwise we might reserve a segment of size 3.
			// Then if we have to release that segment and it cannot
			// coalesce with either neighbor, we would not have
			// enough space for the 4 required bookkeeping elements
			// for a free segment.
			int minReservationSize = count + OverheadPerFreeSegment,
				segmentAddr;
			if (!strategy(this, minReservationSize, out segmentAddr, out holesExamined))
				return false;

			// Make sure the strategy didn't give us an invalid segment.
			if (segmentAddr == NullPointer)
				throw new InvalidOperationException("Memory strategy returned a null pointer but did not indicate error.");
			if (mainMemory[segmentAddr] > 0)
				throw new InvalidOperationException("Memory strategy returned an already reserved segment.");
			int segmentSize = -mainMemory[segmentAddr];
			if (minReservationSize > segmentSize)
				throw new InvalidOperationException("Memory strategy returned a segment of insufficient size.");
			

			// Create a new segment with the remaining free space (if any),
			int remainingSize = segmentSize - minReservationSize;
			if (remainingSize >= OverheadPerFreeSegment)
			{
				// Reserve the back of the original segment.
				int reservedAddr = segmentAddr + remainingSize;
				mainMemory[reservedAddr] = minReservationSize;
				mainMemory[EndTagAddr(reservedAddr)] = minReservationSize;
				ClearMemory(reservedAddr + 1, minReservationSize - OverheadPerReservedSegment);

				allocation = reservedAddr + OffsetToFirstUsableWord;

				// Maintain the free space at the front of the original segment.
				// This allows us to ignore any potential manipulation of the
				// linked list because the free node is in the same position
				// and has the same pointers as the original segment.
				int freeAddr = segmentAddr;
				mainMemory[freeAddr] = -remainingSize;
				mainMemory[EndTagAddr(freeAddr)] = -remainingSize;
				ClearMemory(freeAddr + 3, remainingSize - OverheadPerFreeSegment);
			}
			else
			{
				allocation = segmentAddr + OffsetToFirstUsableWord;

				UnlinkNode(segmentAddr);

				// Mark the segment as reserved.
				mainMemory[segmentAddr] = segmentSize;
				mainMemory[EndTagAddr(segmentAddr)] = segmentSize;
				ClearMemory(segmentAddr + 1, segmentSize - OverheadPerReservedSegment);
			}
			AssertStateCorrect();
			return true;
		}

		public void Release(int allocation)
		{
			AssertStateCorrect();

			int
				segmentAddr = allocation - OffsetToFirstUsableWord,
				leftNeighborAddr,
				rightNeighborAddr;

			if (mainMemory[segmentAddr] < 0)
				throw new ArgumentException("Attempted to release non-reserved memory.", "allocation");

			bool
				leftNeighborFree = LeftNeighborAddr(segmentAddr, out leftNeighborAddr) && mainMemory[leftNeighborAddr] < 0,
				rightNeighborFree = RightNeighborAddr(segmentAddr, out rightNeighborAddr) && mainMemory[rightNeighborAddr] < 0;

			Debug.Assert(headFreeSegment < 0 || mainMemory[headFreeSegment] < 0, "Head free segment was not marked as free.");

			int coalescedAddr, coalescedSize, nextAddr, prevAddr;

			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			if (!leftNeighborFree && !rightNeighborFree)
			{
				// Case 0: If neither are free, add this segment to
				// the head of the linked list.
				coalescedSize = mainMemory[segmentAddr];
				coalescedAddr = segmentAddr;
				if (headFreeSegment == NullPointer)
					nextAddr = prevAddr = coalescedAddr;
				else
				{
					nextAddr = headFreeSegment;
					prevAddr = mainMemory[PrevPtrAddr(headFreeSegment)];
				}
				headFreeSegment = segmentAddr;
			}
			else if (!leftNeighborFree && rightNeighborFree)
			{
				// Case 1: If only right neighbor is free, coalesce
				// the right neighbor into this segment, preserving
				// the right segment's linked list pointers.
				Debug.Assert(mainMemory[segmentAddr] > 0);
				Debug.Assert(mainMemory[rightNeighborAddr] < 0);
				coalescedAddr = segmentAddr;
				coalescedSize = mainMemory[segmentAddr] - mainMemory[rightNeighborAddr];
				nextAddr = mainMemory[NextPtrAddr(rightNeighborAddr)];
				if (nextAddr == rightNeighborAddr)
					nextAddr = coalescedAddr;
				prevAddr = mainMemory[PrevPtrAddr(rightNeighborAddr)];
				if (prevAddr == rightNeighborAddr)
					prevAddr = coalescedAddr;
				if (headFreeSegment == rightNeighborAddr)
					headFreeSegment = segmentAddr;
			}
			else if (leftNeighborFree && !rightNeighborFree)
			{
				// Case 2: If only left neighbor is free, coalesce
				// this segment into the left neighbor, preserving the
				// left segment's linked list pointers.
				Debug.Assert(mainMemory[segmentAddr] > 0);
				Debug.Assert(mainMemory[leftNeighborAddr] < 0);
				coalescedAddr = leftNeighborAddr;
				coalescedSize = mainMemory[segmentAddr] - mainMemory[leftNeighborAddr];
				nextAddr = mainMemory[NextPtrAddr(leftNeighborAddr)];
				prevAddr = mainMemory[PrevPtrAddr(leftNeighborAddr)];
			}
			else // if (leftNeighborFree && rightNeighborFree)
			{
				// Case 3: If both neighbors are free, coalesce this
				// segment with both neighbors. This is the most 
				// involved case because the linked list being 
				// unordered means the left and right segments
				// might not be adjacent in the linked list.
				// We solve this problem by first deleting the right
				// neighbor from the linked list, then expanding
				// the left neighbor to include the newly freed
				// segment and the right neighbor segment.
				Debug.Assert(headFreeSegment != 3);
				Debug.Assert(mainMemory[segmentAddr] > 0);
				Debug.Assert(mainMemory[leftNeighborAddr] < 0);
				Debug.Assert(mainMemory[rightNeighborAddr] < 0);
				UnlinkNode(rightNeighborAddr);
				coalescedAddr = leftNeighborAddr;
				coalescedSize = -mainMemory[leftNeighborAddr] + mainMemory[segmentAddr] - mainMemory[rightNeighborAddr];
				nextAddr = mainMemory[NextPtrAddr(leftNeighborAddr)];
				prevAddr = mainMemory[PrevPtrAddr(leftNeighborAddr)];
			}
			// ReSharper restore ConditionIsAlwaysTrueOrFalse

			mainMemory[coalescedAddr] = -coalescedSize;
			mainMemory[EndTagAddr(coalescedAddr)] = -coalescedSize;
			mainMemory[NextPtrAddr(coalescedAddr)] = nextAddr;
			mainMemory[PrevPtrAddr(coalescedAddr)] = prevAddr;
			Debug.Assert(nextAddr != NullPointer);
			mainMemory[PrevPtrAddr(nextAddr)] = coalescedAddr;
			Debug.Assert(prevAddr != NullPointer);
			mainMemory[NextPtrAddr(prevAddr)] = coalescedAddr;
			ClearMemory(coalescedAddr + 3, coalescedSize - OverheadPerFreeSegment);

			Debug.Assert(headFreeSegment != 3);
			Debug.Assert(headFreeSegment < 0 || mainMemory[headFreeSegment] < 0);

			AssertStateCorrect();
		}

		private int EndTagAddr(int segmentAddr)
		{
			return segmentAddr + Math.Abs(mainMemory[segmentAddr]) - 1;
		}

		public int NextPtrAddr(int segmentAddr)
		{
			return segmentAddr + 2;
		}

		public int PrevPtrAddr(int segmentAddr)
		{
			return segmentAddr + 1;
		}

		private bool LeftNeighborAddr(int segmentAddr, out int leftNeighborAddr)
		{
			leftNeighborAddr = NullPointer;
			if (segmentAddr <= 0)
				return false;
			leftNeighborAddr = segmentAddr - Math.Abs(mainMemory[segmentAddr - 1]);
			return leftNeighborAddr >= 0;
		}

		private bool RightNeighborAddr(int segmentAddr, out int rightNeighborAddr)
		{
			rightNeighborAddr = NullPointer;
			if (segmentAddr >= mainMemory.Length)
				return false;
			rightNeighborAddr = segmentAddr + Math.Abs(mainMemory[segmentAddr]);
			return rightNeighborAddr < mainMemory.Length;
		}

		private void UnlinkNode(int segmentAddr)
		{
			if (headFreeSegment == segmentAddr)
				headFreeSegment = mainMemory[NextPtrAddr(segmentAddr)];
			mainMemory[NextPtrAddr(mainMemory[PrevPtrAddr(segmentAddr)])] = mainMemory[NextPtrAddr(segmentAddr)];
			mainMemory[PrevPtrAddr(mainMemory[NextPtrAddr(segmentAddr)])] = mainMemory[PrevPtrAddr(segmentAddr)];
			if (headFreeSegment == segmentAddr)
				headFreeSegment = NullPointer;
		}

		[Conditional("DEBUG")]
		private void AssertStateCorrect()
		{
			// Check linked list state.
			TraverseFreeList(headFreeSegment, current =>
			{
				Debug.Assert(mainMemory[current] != 0, "Linked list node points to element with value 0.");
				// Make sure next and prev pointers both non-null.
				Debug.Assert(mainMemory[NextPtrAddr(current)] != NullPointer, "A node on the free list had a null next pointer.");
				Debug.Assert(mainMemory[PrevPtrAddr(current)] != NullPointer, "A node on the free list had a null prev pointer.");
				// Make sure next and prev pointers are mutually correct.
				Debug.Assert(mainMemory[NextPtrAddr(mainMemory[PrevPtrAddr(current)])] == current, "Inconsistent linked list state.");
				Debug.Assert(mainMemory[PrevPtrAddr(mainMemory[NextPtrAddr(current)])] == current, "Inconsistent linked list state.");
				// Make sure current node marked as free.
				Debug.Assert(mainMemory[current] < 0, "A node on the free list was not marked as free.");
				// Make sure start and end tag are equal.
				Debug.Assert(mainMemory[current] == mainMemory[EndTagAddr(current)], "Inconsistent start and end tag values.");
				return true;
			});
		}

		private void ClearMemory(int start, int count)
		{
			for (int i = 0; i < count; i++)
				mainMemory[start + i] = 0;
		}

		public int TraverseFreeList(int startNode, Func<int, bool> visitor)
		{
			int segmentsInspected = 0;
			if (startNode == NullPointer)
				return segmentsInspected;
			Debug.Assert(mainMemory[startNode] != 0, "Start node's value was 0.");
			int currentNode = startNode;
			do
			{
				segmentsInspected++;
			}
			while (visitor(currentNode) && (currentNode = mainMemory[NextPtrAddr(currentNode)]) != startNode);
			return segmentsInspected;
		}
	}
}