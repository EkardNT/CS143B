using System;
namespace Project3
{
	public class CreateCommand
	{
		public string LogicalName;
	}

	public class DestroyCommand
	{
		public string LogicalName;
	}

	public class OpenCommand
	{
		public string LogicalName;
	}

	public class CloseCommand
	{
		public int FileHandle;
	}

	public class ReadCommand
	{
		public int FileHandle;
		public int Count;
	}

	public class WriteCommand
	{
		public int FileHandle;
		public int Count;
		public byte Data;
	}

	public class SeekCommand
	{
		public int FileHandle;
		public int Position;
	}

	public class DirectoryCommand
	{
	}

	public class InitCommand
	{
		public string SerializationFilePath;
	}

	public class SaveCommand
	{
		public string SerializationFilePath;
	}

	public class HelpCommand
	{

	}
}