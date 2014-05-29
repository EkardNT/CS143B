using System.Collections;

namespace Project3
{
	public class BlockBitmap
	{
		private readonly int searchStart;
		private readonly BitArray bits;

		public BlockBitmap(IDisk disk, int bitmapBlockCount, int descriptorBlockCount)
		{
			searchStart = bitmapBlockCount + descriptorBlockCount;
			bits = new BitArray(disk.BlockCount, false);
			for (int i = 0; i < searchStart; i++)
				bits[i] = true;
		}

		public int ReserveBlock()
		{
			for(int i = searchStart; i < bits.Length; i++)
				if (!bits[i])
				{
					bits[i] = true;
					return i;
				}
			throw new FileSystemException("Cannot reserve block because there are no free blocks.");
		}

		public void FreeBlock(int block)
		{
			if (block < searchStart)
				throw new FileSystemException("Cannot free a special file system reserved block.");
			if (!bits[block])
				throw new FileSystemException("Cannot free a block which is already free.");
			bits[block] = false;
		}
	}
}