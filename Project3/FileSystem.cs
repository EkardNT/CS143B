using System;
using System.Collections.Generic;

namespace Project3
{
	public unsafe class FileSystem
	{
		#region Constants

		private const int
			// Maximum number of files that can be open at once,
			// aka the number of entries in the open file table.
			// Because the directory must always be open, the actual
			// number of files that the user can open at one time
			// is one less than this value.
			MaxOpenFiles = 4,
			// Number of blocks in the logical disk.
			BlockCount = 64,
			// Size of a single block in bytes.
			BlockSize = 64,
			// Maximum number of files that can be open at once,
			// aka the number of entries in the open file table.
			// Because the directory must always be open, the actual
			// number of files that the user can open at one time
			// is one less than this value.
			MaxFileNameLength = 3,
			// Maximum number of blocks that can be allocated to a single file.
			MaxBlocksPerFile = 3,
			// Number of blocks required for the bitmap. The part after the addition
			// accounts for when the BlockSize does not evenly divide the BlockCount.
			NumBitmapBlocks = BlockCount / BlockSize + (BlockCount % BlockSize == 0 ? 0 : 1),
			// Maximum number of files that can be open at once,
			// aka the number of entries in the open file table.
			// Because the directory must always be open, the actual
			// number of files that the user can open at one time
			// is one less than this value.
			// Maximum possible length of a file in bytes. This does not include the size
			// of the file descriptor, only the number of bytes usable for actual storage
			// of data.
			MaxFileLength = MaxBlocksPerFile * BlockSize,
			// Directory descriptor always in known location.
			DirectoryDescriptorIndex = 0,
			// Size of a descriptor in bytes.
			DescriptorSize = sizeof (int) + sizeof (byte) * MaxBlocksPerFile,
			// The maximum possible number of file descriptors, which will be reached 
			// when the directory file is full.
			MaxDescriptorCount = MaxFileLength / DescriptorSize,
			// Number of blocks required to store all descriptors.
			NumDescriptorBlocks =
				MaxDescriptorCount * DescriptorSize / BlockSize + ((MaxDescriptorCount * DescriptorSize) % BlockSize == 0 ? 0 : 1),
			// Size of a directory entry in bytes.
			DirectoryEntrySize = sizeof (int) + sizeof (byte) * MaxFileNameLength;

		#endregion

		#region Utility Classes/Structs

		private class Descriptor
		{
			// Length in bytes of the file.
			public int Length;
			// Map containing the logical disk buffers that contain
			// the parts of the file.
			public int[] DiskMap { get; private set; }

			public Descriptor()
			{
				Length = -1;
				DiskMap = new int[MaxBlocksPerFile];
				for (int i = 0; i < MaxBlocksPerFile; i++)
					DiskMap[i] = -1;
			}
		}

		private class DirectoryEntry
		{
			// Index of the descriptor describing this file.
			public int DescriptorIndex;
			// Name in UTF-8 encoding, without any C-string trailing NULL byte.
			public byte[] Name { get; private set; }

			public DirectoryEntry()
			{
				DescriptorIndex = -1;
				Name = new byte[MaxFileNameLength];
				for (int i = 0; i < MaxFileNameLength; i++)
					Name[i] = 0;
			}
		}

		private class OpenFileTableEntry
		{
			/// <summary>
			/// The read/write buffer.
			/// </summary>
			public byte[] Buffer { get; private set; }

			/// <summary>
			/// The data pointer for the current file. This is the
			/// absolute byte position within the file, not the position
			/// within the block currently contained within the Buffer,
			/// and not the physical address on disk.
			/// </summary>
			public int DataPointer { get; set; }

			/// <summary>
			/// Index of the current file's descriptor. Is -1 to indicate
			/// this entry in the open file table is empty.
			/// </summary>
			public int Descriptor { get; set; }

			public int DirectoryEntry { get; set; }

			public OpenFileTableEntry(LogicalDisk disk)
			{
				Buffer = new byte[disk.BlockSize];
				DataPointer = -1;
				Descriptor = -1;
				DirectoryEntry = -1;
			}
		}

		#endregion

		#region Fields

		private LogicalDisk disk;
		private OpenFileTableEntry[] openFileTable;

		#endregion

		public FileSystem()
		{
			InitEmpty();
		}

		/// <summary>
		/// Creates a new file with the given logical file name.
		/// </summary>
		public void Create(string fileName)
		{

		}

		/// <summary>
		/// Destroys the file with the given logical file name.
		/// </summary>
		public void Destroy(string fileName)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Opens a file for reading and writing, give the file's
		/// logical name. Returns the file handle which identifies
		/// the newly opened file.
		/// </summary>
		public int Open(string fileName)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Closes a file given its file handle.
		/// </summary>
		public void Close(int file)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Reads a sequence of bytes from a file into a destination buffer.
		/// The file's data pointer is advanced by the number of bytes read.
		/// Returns the number of bytes read.
		/// </summary>
		public int Read(int file, byte[] destination, int count)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Writes a sequence of bytes to a file from a source buffer. The
		/// file's data pointer by the number of bytes written. Returns the
		/// number of bytes written.
		/// </summary>
		public int Write(int file, byte[] source, int count)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Updates a file's data pointer to the given position value.
		/// </summary>
		public void Seek(int file, int position)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the logical names of all currently existent files.
		/// </summary>
		public IEnumerable<string> Directory()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Resets the file system's state to its initial value. Optionally
		/// restores state from a serialization file created by a previous call
		/// to Save() if the given serializationFilePath is not null.
		/// </summary>
		public void Init(string serializationFilePath = null)
		{
			if (serializationFilePath == null)
				InitEmpty();
			else
				InitFile(serializationFilePath);
		}

		/// <summary>
		/// Creates a serialization file at the given path that contains current
		/// state of the file system, suitable for restoring later by calling Init()
		/// with the same file path.
		/// </summary>
		public void Save(string serializationFilePath)
		{
			throw new NotImplementedException();
		}

		private void InitEmpty()
		{
			//
		}

		private void InitFile(string serializationFilePath)
		{
			throw new NotImplementedException();
		}
	}
}