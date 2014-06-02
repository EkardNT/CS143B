using System;

namespace Project3
{
	public class Disk
	{
		private readonly byte[,] disk;

		public Disk(int blockCount, int blockSize)
		{
			disk = new byte[blockCount, blockSize];
			BlockCount = blockCount;
			BlockSize = blockSize;
			for(int i = 0; i < blockCount; i++)
				for (int j = 0; j < blockSize; j++)
					disk[i, j] = 0;
		}

		public int BlockCount { get; private set; }

		public int BlockSize { get; private set; }

		public void ReadBlock(int block, byte[] destination)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");
			if (destination.Length < BlockSize)
				throw new ArgumentException("Destination length is insufficient.", "destination");
			for (int i = 0; i < BlockSize; i++)
				destination[i] = disk[block, i];
		}

		public bool AllowBitmapWrites { get; set; }

		public void WriteBlock(int block, byte[] source)
		{
			if (block == 0 && !AllowBitmapWrites)
				throw new FileSystemException("Writing to bitmap is not allowed.");
			if (source == null)
				throw new ArgumentNullException("source");
			if (source.Length < BlockSize)
				throw new ArgumentException("Source length is insufficient.", "source");
			for (int i = 0; i < BlockSize; i++)
				disk[block, i] = source[i];
		}
	}
}