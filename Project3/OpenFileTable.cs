using System;

namespace Project3
{
	public class OpenFileTable
	{
		private const int DirectoryHandle = 0;

		public interface IEntry
		{
			/// <summary>
			/// The read/write buffer.
			/// </summary>
			byte[] Buffer { get; }
			/// <summary>
			/// The block which is currently loaded into the read/write buffer.
			/// If -1 then no block is currently loaded into the read/write buffer.
			/// </summary>
			int Block { get; set; }
			/// <summary>
			/// The data pointer for the current file. This is the
			/// absolute byte position within the file, not the position
			/// within the block currently contained within the Buffer,
			/// and not the physical address on disk.
			/// </summary>
			int DataPointer { get; set; }
			/// <summary>
			/// Index of the current file's descriptor.
			/// </summary>
			int DescriptorIndex { get; }
			/// <summary>
			/// Index of the current file's directory entry.
			/// </summary>
			int FileIndex { get; }
			/// <summary>
			/// Index into the open file table for this entry.
			/// </summary>
			int FileHandle { get; }
		}

		private class Entry : IEntry
		{
			public byte[] Buffer { get; private set; }
			public int Block { get; set; }
			public int DataPointer { get; set; }
			public int DescriptorIndex { get; set; }
			public int FileIndex { get; set; }
			public int FileHandle { get; private set; }

			public Entry(IDisk disk, int fileHandle)
			{
				Buffer = new byte[disk.BlockSize];
				Block = -1;
				DataPointer = 0;
				DescriptorIndex = -1;
				FileIndex = -1;
				FileHandle = fileHandle;
			}
		}

		private readonly Entry[] entries;

		public OpenFileTable(IDisk disk, int maxOpenFiles)
		{
			if (disk == null) throw new ArgumentNullException("disk");
			if (maxOpenFiles < 1) throw new ArgumentOutOfRangeException("maxOpenFiles");

			entries = new Entry[maxOpenFiles];
			for (int i = 0; i < maxOpenFiles; i++)
				entries[i] = new Entry(disk, i);
		}

		public IEntry this[int fileHandle]
		{
			get
			{
				if (fileHandle == DirectoryHandle)
					throw new FileSystemException(
						"Cannot access the directory file entry by its file handle, use the Directory property instead.");
				return entries[fileHandle];
			}
		}

		public IEntry Directory
		{
			get { return entries[DirectoryHandle]; }
		}

		public int Reserve(int descriptorIndex, int fileIndex)
		{
			if (descriptorIndex < 0)
				throw new ArgumentOutOfRangeException("descriptorIndex");
			if (fileIndex < 0)
				throw new ArgumentOutOfRangeException("fileIndex");
			for(int i = 1; i < entries.Length; i++)
				if (entries[i].DescriptorIndex == -1)
					return i;
			throw new FileSystemException("Cannot reserve an entry in the open file table because the table is full.");
		}

		public void Release(int fileHandle)
		{
			if (entries[fileHandle].DescriptorIndex == -1)
				throw new FileSystemException("Cannot release an open file table file handle that is not reserved.");
			var entry = entries[fileHandle];
			entry.Block = -1;
			entry.DataPointer = 0;
			entry.DescriptorIndex = -1;
			entry.FileIndex = -1;
		}
	}
}