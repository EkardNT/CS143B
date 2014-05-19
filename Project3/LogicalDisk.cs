using System;
namespace Project3
{
	public interface ILogicalDisk
	{
		/// <summary>
		/// Gets the number of logical blocks in the logical disk.
		/// </summary>
		int BlockCount { get; }
		/// <summary>
		/// Gets the size in bytes of a logical block.
		/// </summary>
		int BlockSize { get; }
		/// <summary>
		/// Reads data from a logical block into the given destination
		/// buffer. The destination buffer must be at least as long
		/// as the BlockSize.
		/// </summary>
		void ReadBlock(int block, byte[] destination);
		/// <summary>
		/// Writes data to a logical block from the given source buffer.
		/// The source buffer must be at least as long as the BlockSize.
		/// </summary>
		void WriteBlock(int block, byte[] source);
	}

	public class LogicalDisk : ILogicalDisk
	{
		private readonly byte[,] disk;

		public LogicalDisk(int blockCount, int blockSize)
		{
			disk = new byte[blockCount, blockSize];
			BlockCount = blockCount;
			BlockSize = blockSize;
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

		public void WriteBlock(int block, byte[] source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (source.Length < BlockSize)
				throw new ArgumentException("source length is insufficient.", "destination");
			for (int i = 0; i < BlockSize; i++)
				disk[block, i] = source[i];
		}
	}
}