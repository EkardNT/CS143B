using System.Collections;

namespace Project3
{
	public class OccupancyBitmap
	{
		private readonly BitArray bits;

		public OccupancyBitmap(IDisk disk)
		{
			bits = new BitArray(disk.BlockCount, false);
			RequiredBlocksCount = disk.BlockCount / disk.BlockSize + (disk.BlockCount % disk.BlockSize == 0 ? 0 : 1);
		}

		public int RequiredBlocksCount { get; private set; }

		public void SetReserved(int block)
		{
			if (bits[block])
				throw new FileSystemException("Cannot set reserved a block which is already reserved.");
			bits[block] = true;
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
			if (!bits[block])
				throw new FileSystemException("Cannot release a block which is already free.");
			bits[block] = false;
		}
	}
}