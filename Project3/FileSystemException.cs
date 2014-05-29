using System;

namespace Project3
{
	public class FileSystemException : Exception
	{
		public FileSystemException(string message) : base(message)
		{
		}
	}
}