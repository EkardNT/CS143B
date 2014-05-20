using System.Collections.Generic;
namespace Project3
{
	public class FileSystem
	{
		private const int MaxOpenFiles = 4;

		private LogicalDisk disk;
		private OpenFileTableEntry[] openFileTable;

		public FileSystem(int blockCount, int blockSize)
		{
			disk = new LogicalDisk(blockCount, blockSize);
			openFileTable = new OpenFileTableEntry[MaxOpenFiles];
			for (int i = 0; i < MaxOpenFiles; i++)
				openFileTable[i] = new OpenFileTableEntry(blockSize);
		}

		/// <summary>
		/// Creates a new file with the given logical file name.
		/// The file name must not be longer than 4 bytes when
		/// encoded in UTF-8.
		/// </summary>
		public void Create(string fileName)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Destroys the file with the given logical file name.
		/// </summary>
		public void Destroy(string fileName)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Opens a file for reading and writing, give the file's
		/// logical name. Returns the file handle which identifies
		/// the newly opened file.
		/// </summary>
		public int Open(string fileName)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Closes a file given its file handle.
		/// </summary>
		public void Close(int file)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Reads a sequence of bytes from a file into a destination buffer.
		/// The file's data pointer is advanced by the number of bytes read.
		/// Returns the number of bytes read.
		/// </summary>
		public int Read(int file, byte[] destination, int count)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Writes a sequence of bytes to a file from a source buffer. The
		/// file's data pointer by the number of bytes written. Returns the
		/// number of bytes written.
		/// </summary>
		public int Write(int file, byte[] source, int count)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Updates a file's data pointer to the given position value.
		/// </summary>
		public void Seek(int file, int position)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Returns the logical names of all currently existent files.
		/// </summary>
		public IEnumerable<string> Directory()
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Resets the file system's state to its initial value. Optionally
		/// restores state from a serialization file created by a previous call
		/// to Save() if the given serializationFilePath is not null.
		/// </summary>
		public void Init(string serializationFilePath = null)
		{
			throw new System.NotImplementedException();
		}
		/// <summary>
		/// Creates a serialization file at the given path that contains current
		/// state of the file system, suitable for restoring later by calling Init()
		/// with the same file path.
		/// </summary>
		public void Save(string serializationFilePath)
		{
			throw new System.NotImplementedException();
		}
	}

	public class OpenFileTableEntry
	{
		/// <summary>
		/// The read/write buffer containing one full block.
		/// </summary>
		public byte[] Buffer { get; private set; }
		/// <summary>
		/// The data pointer for the current file. This is the
		/// absolute position within the file, not the position
		/// within the block currently contained within the Buffer.
		/// </summary>
		public int DataPointer { get; set; }
		/// <summary>
		/// Index of the current file's descriptor.
		/// </summary>
		public int Descriptor { get; set; }

		public OpenFileTableEntry(int blockSize)
		{
			Buffer = new byte[blockSize];
			DataPointer = -1;
			Descriptor = -1;
		}
	}
}