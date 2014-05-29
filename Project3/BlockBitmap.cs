using System.Collections;

namespace Project3
{
	public class BlockBitmap
	{
		private readonly BitArray bits, special;

		public BlockBitmap(IDisk disk)
		{
			bits = new BitArray(disk.BlockCount, false);
			special = new BitArray(disk.BlockCount, false);
			// Mark the blocks required for the bitmap itself as special.
			RequiredBlocksCount = disk.BlockCount / disk.BlockSize + (disk.BlockCount % disk.BlockSize == 0 ? 0 : 1);
			for (int i = 0; i < RequiredBlocksCount; i++)
				ReserveSpecial(i);
		}

		public int RequiredBlocksCount { get; private set; }

		public void ReserveSpecial(int block)
		{
			if (bits[block])
				throw new FileSystemException("Cannot specially reserve block because it is already reserved.");
			bits[block] = special[block] = true;
		}

		public int Reserve()
		{
			for(int i = 0; i < bits.Length; i++)
				if (!bits[i])
				{
					bits[i] = true;
					return i;
				}
			throw new FileSystemException("Cannot reserve block because there are no free blocks.");
		}

		public void Release(int block)
		{
			if (special[block])
				throw new FileSystemException("Cannot release a special file-system-reserved block.");
			if (!bits[block])
				throw new FileSystemException("Cannot release a block which is already free.");
			bits[block] = false;
		}
	}
}