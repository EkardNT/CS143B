using System;
using System.Collections.Generic;

namespace Project3
{
	public class FileSystem
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
			BitmapBlocksCount = BlockCount / BlockSize + (BlockCount % BlockSize == 0 ? 0 : 1),
			// Maximum number of files that can be open at once,
			// aka the number of entries in the open file table.
			// Because the directory must always be open, the actual
			// number of files that the user can open at one time
			// is one less than this value.
			// Maximum possible length of a file in bytes. This does not include the size
			// of the file descriptor, only the number of bytes usable for actual storage
			// of data.
			MaxFileLength = MaxBlocksPerFile * BlockSize,
			// Size of a descriptor in bytes.
			DescriptorSize = sizeof (int) + sizeof (byte) * MaxBlocksPerFile,
			// The maximum possible number of file descriptors, which will be reached 
			// when the directory file is full.
			MaxDescriptorCount = MaxFileLength / DescriptorSize,
			// Number of blocks required to store all descriptors.
			DescriptorBlocksCount =
				MaxDescriptorCount * DescriptorSize / BlockSize + ((MaxDescriptorCount * DescriptorSize) % BlockSize == 0 ? 0 : 1),
			// Size of a directory entry in bytes.
			DirectoryEntrySize = sizeof (int) + sizeof (byte) * MaxFileNameLength;

		#endregion
		
		private IDisk disk;
		private BlockBitmap bitmap;
		private OpenFileTable oft;

		public FileSystem()
		{
			InitEmpty();
		}

		#region File System API Methods

		/// <summary>
		/// Creates a new file with the given logical file name.
		/// </summary>
		public void Create(string fileName)
		{
			throw new NotImplementedException();
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
				InitFromFile(serializationFilePath);
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

		#endregion

		#region Helper Methods

		private void InitEmpty()
		{
			// Allocate data structures
			disk = new MemoryDisk(BlockCount, BlockSize);
			bitmap = new BlockBitmap(disk);
			oft = new OpenFileTable(disk, MaxOpenFiles);

			// Create directory file descriptor.
		}

		private void InitFromFile(string serializationFilePath)
		{
			throw new NotImplementedException();
		}
		
		#endregion
	}
}