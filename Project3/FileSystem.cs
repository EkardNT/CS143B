using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Project3
{
	public class FileSystem
	{
		#region Constants

		public const int
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
			// Maximum byte length of a file name encoded in UTF-8.
			MaxFileNameLength = 3,
			MaxBlocksPerFile = 3,
			MaxFileLength = MaxBlocksPerFile * BlockSize,
			// Note: have to reserve one extra byte for trailing null character
			// to accomodate C strings.
			DirectoryEntrySize = sizeof(int) + (MaxFileNameLength + 1) * sizeof(byte),
			DirectoryEntryCount = MaxFileLength / DirectoryEntrySize,
			DescriptorSize = (1 + MaxBlocksPerFile) * sizeof(int),
			DescriptorCount = DirectoryEntryCount,
			DescriptorBlockCount = DescriptorCount * DescriptorSize / BlockSize + (DescriptorCount * DescriptorSize % BlockSize == 0 ? 0 : 1),
			BitmapBlockCount = BlockCount / BlockSize + (BlockCount % BlockSize == 0 ? 0 : 1);

		#endregion

		private class OftEntry
		{
			// Index of the descriptor that describes the 
			// file open in this open file table entry, or
			// -1 if no file is open.
			public int DescriptorIndex;
			// The offset into the file that the next byte
			// will be read from or written to. Starts at 0.
			public int DataPointer;
			// The file index of the block that is currently
			// loaded into the buffer, or -1 if no block is
			// loaded. This is the block offset in the file, 
			// not the index of the physical block on disk
			// (to get that, consult the descriptor's disk map).
			public int FileBlock;
			// The file data buffer, containing one full block.
			public byte[] Buffer { get; private set; }

			public OftEntry()
			{
				DescriptorIndex = FileBlock = -1;
				DataPointer = 0;
				Buffer = new byte[BlockSize];
				for (int i = 0; i < BlockSize; i++)
					Buffer[i] = 0;
			}
		}

		private class Descriptor
		{
			// -1 if the descriptor is unreserved.
			public int Length;
			// Maps file blocks to disk block indexes.
			public int[] DiskMap { get; private set; }

			public Descriptor()
			{
				Length = -1;
				DiskMap = new int[MaxBlocksPerFile];
				for (int i = 0; i < MaxBlocksPerFile; i++)
					DiskMap[i] = -1;
			}
		}

		private class DirEntry
		{
			// -1 if this entry is empty.
			public int DescriptorIndex;
			public byte[] Name;

			public DirEntry()
			{
				DescriptorIndex = -1;
				Name = new byte[MaxFileNameLength];
				for (int i = 0; i < MaxFileNameLength; i++)
					Name[i] = 0;
			}
		}

		private Disk disk;
		private OftEntry[] oft;

		public FileSystem()
		{
			Init();
		}

		#region File System API Methods

		/// <summary>
		/// Creates a new file with the given logical file name.
		/// </summary>
		public void Create(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new FileSystemException("Cannot create a file with a null, empty, or pure whitespace name.");
			var nameBytes = Encoding.UTF8.GetBytes(fileName);
			if (nameBytes.Length > MaxFileNameLength)
				throw new FileSystemException("File name too long.");
			int unused1, unused2;
			if (SearchDirectory(nameBytes, out unused1, out unused2))
				throw new FileSystemException("Duplicate file name.");

			int descriptorIndex = FindFreeDescriptor();
			var descriptor = new Descriptor();
			descriptor.Length = 0;
			WriteDescriptor(descriptorIndex, descriptor);

			int dirEntryIndex = FindFreeDirEntry();
			var dirEntry = new DirEntry();
			dirEntry.DescriptorIndex = descriptorIndex;
			for (int i = 0; i < nameBytes.Length; i++)
				dirEntry.Name[i] = nameBytes[i];
			WriteDirEntry(dirEntryIndex, dirEntry);
		}

		/// <summary>
		/// Destroys the file with the given logical file name.
		/// </summary>
		public void Destroy(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new FileSystemException("Cannot destroy a file with a null, empty, or pure whitespace name.");
			var nameBytes = Encoding.UTF8.GetBytes(fileName);
			if (nameBytes.Length > MaxFileNameLength)
				throw new FileSystemException("File name too long.");

			int descriptorIndex, directoryEntryIndex;
			if (!SearchDirectory(nameBytes, out directoryEntryIndex, out descriptorIndex))
				throw new FileSystemException("No such file exists.");
			if (descriptorIndex == 0)
				throw new FileSystemException("Cannot delete system directory file.");

			// Close open file table if neccessary.
			int fileHandle;
			if (SearchOpenFileTable(descriptorIndex, out fileHandle))
				Close(fileHandle);

			// Clear the entry from the directory file.
			var dirEntry = new DirEntry();
			WriteDirEntry(directoryEntryIndex, dirEntry);

			// Destroy descriptor.
			var descriptor = ReadDescriptor(descriptorIndex);
			// Free reserved blocks.
			for(int i = 0; i < MaxBlocksPerFile; i++)
			{
				// Break as soon as we find the first non-reserved index.
				if (descriptor.DiskMap[i] == -1)
					break;
				SetBitmapBit(descriptor.DiskMap[i], false);
				ClearBlock(descriptor.DiskMap[i]);
				descriptor.DiskMap[i] = -1;
			}
			descriptor.Length = -1;
			WriteDescriptor(descriptorIndex, descriptor);
		}

		/// <summary>
		/// Opens a file for reading and writing, given the file's
		/// logical name. Returns the file handle which identifies
		/// the newly opened file.
		/// </summary>
		public int Open(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new FileSystemException("Cannot open a file with a null, empty, or pure whitespace name.");
			var nameBytes = Encoding.UTF8.GetBytes(fileName);
			if (nameBytes.Length > MaxFileNameLength)
				throw new FileSystemException("File name too long.");

			int descriptorIndex, directoryEntryIndex;
			if (!SearchDirectory(nameBytes, out directoryEntryIndex, out descriptorIndex))
				throw new FileSystemException("No such file exists.");

			int fileHandle;
			if(SearchOpenFileTable(descriptorIndex, out fileHandle))
				throw new FileSystemException("The file is already open.");

			fileHandle = FindFreeFileHandle();
			var entry = oft[fileHandle];
			entry.DescriptorIndex = descriptorIndex;
			entry.FileBlock = -1;
			entry.DataPointer = 0;

			return fileHandle;
		}

		/// <summary>
		/// Closes a file given its file handle.
		/// </summary>
		public void Close(int fileHandle)
		{
			if (fileHandle == 0)
				throw new FileSystemException("Unable to close system directory file.");
			FileClose(fileHandle);
		}

		/// <summary>
		/// Reads a sequence of bytes from a file into a destination buffer.
		/// The file's data pointer is advanced by the number of bytes read.
		/// Returns the number of bytes read.
		/// </summary>
		public int Read(int fileHandle, byte[] destination, int count)
		{
			if (fileHandle == 0)
				throw new FileSystemException("Unable to read from system directory file.");
			return FileRead(fileHandle, destination, count);
		}

		/// <summary>
		/// Writes a sequence of bytes to a file from a source buffer. The
		/// file's data pointer by the number of bytes written. Returns the
		/// number of bytes written.
		/// </summary>
		public int Write(int fileHandle, byte[] source, int count)
		{
			if (fileHandle == 0)
				throw new FileSystemException("Unable to write to system directory file.");
			return FileWrite(fileHandle, source, count);
		}

		/// <summary>
		/// Updates a file's data pointer to the given position value.
		/// </summary>
		public void Seek(int fileHandle, int position)
		{
			if (fileHandle < 0 || fileHandle >= MaxOpenFiles)
				throw new FileSystemException("Attempted to seek with invalid file handle.");
			if (position < 0 || position >= MaxFileLength)
				throw new FileSystemException("Attempted to seek to an illegal position.");
			oft[fileHandle].DataPointer = position;
		}

		/// <summary>
		/// Returns the logical names of all currently existing files.
		/// </summary>
		public IEnumerable<string> Directory()
		{
			var files = new List<string>();

			for (int i = 0; i < DirectoryEntryCount; i++)
			{
				var dirEntry = ReadDirEntry(i);
				if (dirEntry.DescriptorIndex != -1)
					files.Add(Encoding.UTF8.GetString(dirEntry.Name.TakeWhile((val, index) => val != 0).ToArray()));
			}

			return files;
		}

		/// <summary>
		/// Resets the file system's state to its initial value.
		/// </summary>
		public void Init()
		{
			disk = new Disk(BlockCount, BlockSize);
			oft = new OftEntry[MaxOpenFiles];
			for (int i = 0; i < MaxOpenFiles; i++)
				oft[i] = new OftEntry();

			// Mark the blocks needed for the bitmap and
			// the file descriptors as reserved. 
			for (int i = 0; i < BitmapBlockCount + DescriptorBlockCount; i++)
				SetBitmapBit(i, true);

			// Initialize all descriptors to blank.
			var descriptor = new Descriptor();
			for (int i = 1; i < DescriptorCount; i++)
				WriteDescriptor(i, descriptor);

			// Initialize the directory descriptor.
			descriptor.Length = 0;
			WriteDescriptor(0, descriptor);

			// Initialize directory's oft entry.
			var oftDir = oft[0];
			oftDir.DescriptorIndex = 0;

			// Initialize the directory file.
			var dirEntry = new DirEntry();
			for (int i = 1; i < DirectoryEntryCount; i++)
				WriteDirEntry(i, dirEntry);

			// Create the directory entry which describes the
			// directory itself.
			dirEntry.DescriptorIndex = 0;
			dirEntry.Name[0] = (byte)'d';
			dirEntry.Name[1] = (byte)'i';
			dirEntry.Name[2] = (byte)'r';
			WriteDirEntry(0, dirEntry);

			var dirDesc = ReadDescriptor(0);
		}

		/// <summary>
		/// Restores state from a serialization file created by 
		/// a previous call to Save()
		/// </summary>
		public void Load(string serializationFilePath)
		{
			// Clear existing data. This also has the effect
			// of opening the directory file only.
			Init();
			FileStream file;

			try
			{
				file = File.Open(serializationFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			catch(Exception)
			{
				throw new FileSystemException("Unable to open serialization file.");
			}

			using(var reader = new BinaryReader(file, Encoding.UTF8, false))
			{
				var me = reader.ReadString();
				if (me != "Drake Tetreault, UCI ID 35571095")
					throw new FileSystemException("Improperly formatted serialization file.");

				int blockCount = reader.ReadInt32();
				int blockSize = reader.ReadInt32();
				if (blockCount != BlockCount || blockSize != BlockSize)
					throw new FileSystemException(string.Format("Serialization file specified different disk dimensions than BlockSize={0}, BlockCount={1}, which is unsupported.", BlockSize, BlockCount));

				var block = new byte[blockSize];
				for(int i = 0; i < blockCount; i++)
				{
					file.Read(block, 0, blockSize);
					disk.WriteBlock(i, block);
				}
			}
		}

		/// <summary>
		/// Creates a serialization file at the given path that contains current
		/// state of the file system, suitable for restoring later by calling Init()
		/// with the same file path.
		/// </summary>
		public void Save(string serializationFilePath)
		{
			FileStream file;

			try
			{
				file = File.Open(serializationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			}
			catch(Exception)
			{
				throw new FileSystemException("Unable to create serialization file.");
			}

			// Flush all open file buffers and close them.
			for(int i = 0; i < MaxOpenFiles; i++)
			{
				if (oft[i].DescriptorIndex != -1)
					FileClose(i);
			}

			using(var writer = new BinaryWriter(file, Encoding.UTF8, false))
			{
				// Me
				writer.Write("Drake Tetreault, UCI ID 35571095");

				// Dimensions
				writer.Write(BlockCount);
				writer.Write(BlockSize);

				// Disk
				var block = new byte[BlockSize];
				for(int i = 0; i < BlockCount; i++)
				{
					disk.ReadBlock(i, block);
					file.Write(block, 0, BlockSize);
				}
			}
		}

		#endregion

		private void ClearBlock(int blockIndex)
		{
			var data = new byte[BlockSize];
			disk.WriteBlock(blockIndex, data);
		}

		private bool SearchOpenFileTable(int descriptorIndex, out int fileHandle)
		{
			for(int i = 0; i < MaxOpenFiles; i++)
			{
				if(oft[i].DescriptorIndex == descriptorIndex)
				{
					fileHandle = i;
					return true;
				}
			}
			fileHandle = -1;
			return false;
		}

		private bool SearchDirectory(byte[] name, out int directoryEntryIndex, out int descriptorIndex)
		{
			for(int i = 0; i < DirectoryEntryCount; i++)
			{
				var entry = ReadDirEntry(i);
				if (name.Zip(entry.Name, (e1, e2) => new { E1 = e1, E2 = e2 }).All(both => both.E1 == both.E2))
				{
					directoryEntryIndex = i;
					descriptorIndex = entry.DescriptorIndex;
					return true;
				}
			}
			directoryEntryIndex = descriptorIndex = -1;
			return false;
		}

		private void WriteDirEntry(int dirEntryIndex, DirEntry entry)
		{
			var data = new byte[DirectoryEntrySize];
			IntToBytes(data, 0, entry.DescriptorIndex);
			for (int i = 0; i < MaxFileNameLength; i++)
				data[sizeof(int) + i] = entry.Name[i];
			Seek(0, dirEntryIndex * DirectoryEntrySize);
			FileWrite(0, data, DirectoryEntrySize);
		}

		private DirEntry ReadDirEntry(int dirEntryIndex)
		{
			var data = new byte[DirectoryEntrySize];
			Seek(0, dirEntryIndex * DirectoryEntrySize);
			FileRead(0, data, DirectoryEntrySize);
			var entry = new DirEntry();
			entry.DescriptorIndex = BytesToInt(data, 0);
			for (int i = 0; i < MaxFileNameLength; i++)
				entry.Name[i] = data[sizeof(int) + i];
			return entry;
		}

		private void PageOftBuffer(OftEntry entry, Descriptor descriptor, int newBlockIndex)
		{
			if(entry.FileBlock != -1)
				disk.WriteBlock(descriptor.DiskMap[entry.FileBlock], entry.Buffer);
			disk.ReadBlock(descriptor.DiskMap[entry.FileBlock = newBlockIndex], entry.Buffer);
		}

		private void FileClose(int fileHandle)
		{
			if (fileHandle < 0 || fileHandle >= MaxOpenFiles)
				throw new FileSystemException("Attempted to close a file with an invalid file handle.");
			if (oft[fileHandle].DescriptorIndex == -1)
				throw new FileSystemException("Attempted to close an already closed file handle.");

			var entry = oft[fileHandle];
			// Flush buffer to disk.
			if (entry.FileBlock != -1)
			{
				var descriptor = ReadDescriptor(entry.DescriptorIndex);
				disk.WriteBlock(descriptor.DiskMap[entry.FileBlock], entry.Buffer);
			}
			// Clear entry.
			entry.DataPointer = 0;
			entry.DescriptorIndex = -1;
			entry.FileBlock = -1;
		}

		private int FileRead(int fileHandle, byte[] destination, int count)
		{
			if (fileHandle < 0 || fileHandle >= MaxOpenFiles)
				throw new FileSystemException("Attempted to read from an invalid file handle.");
			if (oft[fileHandle].DescriptorIndex == -1)
				throw new FileSystemException("Attempted to read from an invalid file handle.");

			var oftEntry = oft[fileHandle];
			int finalPosition = oftEntry.DataPointer + count;

			var descriptor = ReadDescriptor(oftEntry.DescriptorIndex);
			if (finalPosition > descriptor.Length)
				throw new FileSystemException("Attempted to read beyond the end of a file.");

			for (int i = 0; i < count; i++)
			{
				int filePosition = oftEntry.DataPointer + i;
				int fileBlock = filePosition / BlockSize;
				if (oftEntry.FileBlock != fileBlock)
					PageOftBuffer(oftEntry, descriptor, fileBlock);
				int byteOffset = filePosition % BlockSize;
				destination[i] = oftEntry.Buffer[byteOffset];
			}

			oftEntry.DataPointer = finalPosition;
			return count;
		}

		private int FileWrite(int fileHandle, byte[] source, int count)
		{
			if (fileHandle < 0 || fileHandle >= MaxOpenFiles)
				throw new FileSystemException("Attempted to write to an invalid file handle.");
			if (oft[fileHandle].DescriptorIndex == -1)
				throw new FileSystemException("Attempted to write to an invalid file handle.");

			var oftEntry = oft[fileHandle];
			int finalPosition = oftEntry.DataPointer + count;
			if (finalPosition > MaxFileLength)
				throw new FileSystemException("Attempted to write beyond the maximum size of a file.");

			var descriptor = ReadDescriptor(oftEntry.DescriptorIndex);
			if (finalPosition > descriptor.Length)
				FileResize(oftEntry.DescriptorIndex, descriptor, finalPosition);

			for (int i = 0; i < count; i++)
			{
				int filePosition = oftEntry.DataPointer + i;
				int fileBlock = filePosition / BlockSize;
				if (oftEntry.FileBlock != fileBlock)
					PageOftBuffer(oftEntry, descriptor, fileBlock);
				int byteOffset = filePosition % BlockSize;
				oftEntry.Buffer[byteOffset] = source[i];
			}

			oftEntry.DataPointer = finalPosition;
			return count;
		}

		private void FileResize(int descriptorIndex, Descriptor descriptor, int newLength)
		{
			if (newLength < 0 || newLength > MaxFileLength)
				throw new FileSystemException("Attempted to resize a file to an invalid length.");

			int newBlockCount = newLength / BlockSize + (newLength % BlockSize == 0 ? 0 : 1);

			// Free any extra blocks.
			for(int i = newBlockCount; i < MaxBlocksPerFile; i++)
			{
				// Stop when we reach the first already free block.
				if (descriptor.DiskMap[i] == -1)
					break;
				SetBitmapBit(descriptor.DiskMap[i], false);
				descriptor.DiskMap[i] = -1;
			}
			// Reserve as many as required.
			for(int i = newBlockCount -1; i >= 0; i--)
			{
				// Stop when we reach the first already reserved block.
				if (descriptor.DiskMap[i] != -1)
					break;
				descriptor.DiskMap[i] = FindFreeBlock();
				SetBitmapBit(descriptor.DiskMap[i], true);
			}

			descriptor.Length = newLength;
			WriteDescriptor(descriptorIndex, descriptor);
		}

		private void WriteDescriptor(int descriptorIndex, Descriptor descriptor)
		{
			int blockOffset = BitmapBlockCount + descriptorIndex * DescriptorSize / BlockSize;
			int byteOffset = descriptorIndex * DescriptorSize % BlockSize;
			
			var block = new byte[BlockSize];
			disk.ReadBlock(blockOffset, block);

			IntToBytes(block, byteOffset, descriptor.Length);
			for (int i = 0; i < MaxBlocksPerFile; i++)
				IntToBytes(block, byteOffset + (i + 1) * sizeof(int), descriptor.DiskMap[i]);

			disk.WriteBlock(blockOffset, block);
		}

		private Descriptor ReadDescriptor(int descriptorIndex)
		{
			int blockOffset = BitmapBlockCount + descriptorIndex * DescriptorSize / BlockSize;
			int byteOffset = descriptorIndex * DescriptorSize % BlockSize;

			var block = new byte[BlockSize];
			disk.ReadBlock(blockOffset, block);

			var descriptor = new Descriptor();
			descriptor.Length = BytesToInt(block, byteOffset);
			for (int i = 0; i < MaxBlocksPerFile; i++)
				descriptor.DiskMap[i] = BytesToInt(block, byteOffset + (i + 1) * sizeof(int));

			return descriptor;
		}

		public static void IntToBytes(byte[] bytes, int offset, int value)
		{
			uint uVal = (uint)value;
			bytes[offset + 0] = (byte)(0xFF & (uVal >> 0));
			bytes[offset + 1] = (byte)(0xFF & (uVal >> 8));
			bytes[offset + 2] = (byte)(0xFF & (uVal >> 16));
			bytes[offset + 3] = (byte)(0xFF & (uVal >> 24));
		}

		public static int BytesToInt(byte[] bytes, int offset)
		{
			uint uVal = 0;
			uVal |= (uint)bytes[offset + 0] << 0;
			uVal |= (uint)bytes[offset + 1] << 8;
			uVal |= (uint)bytes[offset + 2] << 16;
			uVal |= (uint)bytes[offset + 3] << 24;
			return (int)uVal;
		}

		private int FindFreeFileHandle()
		{
			for(int i = 0; i < MaxOpenFiles; i++)
			{
				if (oft[i].DescriptorIndex == -1)
					return i;
			}
			throw new FileSystemException("No free file handle found.");
		}
		
		private int FindFreeBlock()
		{
			for(int i = 0; i < BlockCount; i++)
			{
				if (!GetBitmapBit(i))
					return i;
			}
			throw new FileSystemException("No free disk blocks found.");
		}

		private int FindFreeDescriptor()
		{
			for(int i = 0; i < DescriptorCount; i++)
			{
				if (ReadDescriptor(i).Length == -1)
					return i;
			}
			throw new FileSystemException("No free descriptor found.");
		}

		private int FindFreeDirEntry()
		{
			for(int i = 0; i < DirectoryEntryCount; i++)
			{
				if (ReadDirEntry(i).DescriptorIndex == -1)
					return i;
			}
			throw new FileSystemException("No free directory entry found.");
		}

		private void SetBitmapBit(int blockIndex, bool bitValue)
		{
			if (blockIndex < 0 || blockIndex >= BlockCount)
				throw new FileSystemException("Attempted to set bitmap bit outside of legal range.");
				
			int blockOffset = blockIndex / BlockSize;
			int byteOffset = blockIndex / 8;
			int bitOffset = blockIndex % 8;

			var block = new byte[BlockSize];
			disk.ReadBlock(blockOffset, block);

			if (bitValue)
				block[byteOffset] |= (byte)(1 << bitOffset);
			else
				block[byteOffset] &= (byte)~(1 << bitOffset);

			disk.WriteBlock(blockOffset, block);
		}

		private bool GetBitmapBit(int blockIndex)
		{
			if (blockIndex < 0 || blockIndex >= BlockCount)
				throw new FileSystemException("Attempted to set bitmap bit outside of legal range.");
			
			int blockOffset = blockIndex / BlockSize;
			int byteOffset = blockIndex / 8;
			int bitOffset = blockIndex % 8;

			var block = new byte[BlockSize];
			disk.ReadBlock(blockOffset, block);

			return (block[byteOffset] & (byte)(1 << bitOffset)) != 0;
		}
	}
}