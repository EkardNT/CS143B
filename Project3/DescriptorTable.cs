namespace Project3
{
	public class DescriptorTable
	{
		/// <summary>
		/// The maximium number of blocks that can be allocated per descriptor.
		/// </summary>
		public const int MaxBlocksPerDescriptor = 3;
		/// <summary>
		/// The size in bytes of a single descriptor.
		/// </summary>
		public const int DescriptorSize = sizeof (int) * sizeof (int) * MaxBlocksPerDescriptor;

		private const int DirectoryDescriptorIndex = 0;

		public interface IEntry
		{
			/// <summary>
			/// The length of the file described by this descriptor.
			/// </summary>
			int Length { get; }
			/// <summary>
			/// The blocks that contain the data of the file described by
			/// this descriptor, or -1 to indicate the block is not yet used.
			/// Listed in order of data, not in disk order.
			/// </summary>
			int[] DiskMap { get; }
		}

		private class Entry : IEntry
		{
			public int Length { get; set; }
			public int[] DiskMap { get; private set; }

			public Entry()
			{
				Length = -1;
				DiskMap = new int[MaxBlocksPerDescriptor];
				for (int i = 0; i < MaxBlocksPerDescriptor; i++)
					DiskMap[i] = -1;
			}
		}

		private readonly Entry[] descriptors;

		public DescriptorTable(IDisk disk, int descriptorCount)
		{
			descriptors = new Entry[descriptorCount];
			for(int i = 0; i < descriptorCount; i++)
				descriptors[i] = new Entry();
			descriptors[DirectoryDescriptorIndex].Length = 0;
			RequiredBlocksCount = descriptorCount * DescriptorSize / disk.BlockSize +
			                      (descriptorCount * DescriptorSize % disk.BlockSize == 0 ? 0 : 1);
		}

		public int RequiredBlocksCount { get; private set; }

		public IEntry this[int descriptorIndex]
		{
			get
			{
				if (descriptors[descriptorIndex].Length == -1)
					throw new FileSystemException("Cannot access an unreserved descriptor.");
				return descriptors[descriptorIndex];
			}
		}
		
		public IEntry Directory
		{
			get { return descriptors[DirectoryDescriptorIndex]; }
		}

		public int Reserve(int initialLength)
		{
			for(int i = 0; i < descriptors.Length; i++)
				if (descriptors[i].Length == -1)
				{
					descriptors[i].Length = initialLength;
					return i;
				}
			throw new FileSystemException("Cannot reserve a descriptor because there is no available descriptor.");
		}

		public void Release(int descriptorIndex)
		{
			if (descriptorIndex == DirectoryDescriptorIndex)
				throw new FileSystemException("Cannot release the directory descriptor.");
			if (descriptors[descriptorIndex].Length == -1)
				throw new FileSystemException("Cannot release a descriptor which is not reserved.");
			var d = descriptors[descriptorIndex];
			for (int i = 0; i < MaxBlocksPerDescriptor; i++)
				d.DiskMap[i] = -1;
		}

		public void Resize(int descriptorIndex, int newLength)
		{
			if (descriptors[descriptorIndex].Length == -1)
				throw new FileSystemException("Cannot resize an unreserved descriptor.");
			descriptors[descriptorIndex].Length = newLength;
		}
	}
}