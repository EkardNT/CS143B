using System;
using System.IO;
using System.Text;

namespace Project1
{
	public interface IInputSource : IDisposable
	{
		string Name { get; }
		void Init();
		bool MoveNext();
		string CurrentCommand { get; }
	}

	public class ConsoleInputSource : IInputSource
	{
		public string Name
		{
			get { return "system console"; }
		}

		public void Dispose()
		{
		}

		public void Init()
		{
		}

		public bool MoveNext()
		{
			try
			{
				return (CurrentCommand = Console.ReadLine()) != null;
			}
			catch (IOException)
			{
				return false;
			}
		}

		public string CurrentCommand { get; private set; }
	}

	public class ScriptInputSource : IInputSource
	{
		private readonly string filePath;
		private StreamReader reader;

		public ScriptInputSource(string filePath)
		{
			this.filePath = filePath;
		}

		public void Dispose()
		{
			if (reader != null)
			{
				reader.Dispose();
				reader = null;
			}
		}

		public void Init()
		{
			reader = new StreamReader(File.OpenRead(filePath), Encoding.UTF8);
		}

		public bool MoveNext()
		{
			try
			{
				return reader != null && (CurrentCommand = reader.ReadLine()) != null;
			}
			catch (IOException)
			{
				return false;
			}
		}

		public string CurrentCommand { get; private set; }

		public string Name
		{
			get { return string.Format("script file \"{0}\"", Path.GetFileName(filePath)); }
		}
	}
}