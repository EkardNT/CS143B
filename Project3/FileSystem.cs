using System.Collections.Generic;
namespace Project3
{
	public interface IFileSystem
	{
		/// <summary>
		/// Creates a new file with the given logical file name.
		/// The file name must not be longer than 4 bytes when
		/// encoded in UTF-8.
		/// </summary>
		void Create(string fileName);

		/// <summary>
		/// Destroys the file with the given logical file name.
		/// </summary>
		void Destroy(string fileName);

		/// <summary>
		/// Opens a file for reading and writing, give the file's
		/// logical name. Returns the file handle which identifies
		/// the newly opened file.
		/// </summary>
		int Open(string fileName);

		/// <summary>
		/// Closes a file given its file handle.
		/// </summary>
		void Close(int file);

		/// <summary>
		/// Reads a sequence of bytes from a file into a destination buffer.
		/// The file's data pointer is advanced by the number of bytes read.
		/// Returns the number of bytes read.
		/// </summary>
		int Read(int file, byte[] destination, int count);

		/// <summary>
		/// Writes a sequence of bytes to a file from a source buffer. The
		/// file's data pointer by the number of bytes written. Returns the
		/// number of bytes written.
		/// </summary>
		int Write(int file, byte[] source, int count);

		/// <summary>
		/// Updates a file's data pointer to the given position value.
		/// </summary>
		void Seek(int file, int position);

		/// <summary>
		/// Returns the logical names of all currently existent files.
		/// </summary>
		IEnumerable<string> Directory();

		/// <summary>
		/// Resets the file system's state to its initial value. Optionally
		/// restores state from a serialization file created by a previous call
		/// to Save() if the given serializationFilePath is not null.
		/// </summary>
		void Init(string serializationFilePath = null);

		/// <summary>
		/// Creates a serialization file at the given path that contains current
		/// state of the file system, suitable for restoring later by calling Init()
		/// with the same file path.
		/// </summary>
		void Save(string serializationFilePath);
	}

	public class FileSystem : IFileSystem
	{
		public void Create(string fileName)
		{
			throw new System.NotImplementedException();
		}

		public void Destroy(string fileName)
		{
			throw new System.NotImplementedException();
		}

		public int Open(string fileName)
		{
			throw new System.NotImplementedException();
		}

		public void Close(int file)
		{
			throw new System.NotImplementedException();
		}

		public int Read(int file, byte[] destination, int count)
		{
			throw new System.NotImplementedException();
		}

		public int Write(int file, byte[] source, int count)
		{
			throw new System.NotImplementedException();
		}

		public void Seek(int file, int position)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<string> Directory()
		{
			throw new System.NotImplementedException();
		}

		public void Init(string serializationFilePath = null)
		{
			throw new System.NotImplementedException();
		}

		public void Save(string serializationFilePath)
		{
			throw new System.NotImplementedException();
		}
	}
}