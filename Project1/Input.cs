﻿using System;
using System.IO;
using System.Text;

namespace Project1
{
	public interface IInput : IDisposable
	{
		string Name { get; }
		void Init();
		bool MoveNext();
		string CurrentInput { get; }
	}

	public class ConsoleInput : IInput
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
				return (CurrentInput = Console.ReadLine()) != null;
			}
			catch (IOException)
			{
				return false;
			}
		}

		public string CurrentInput { get; private set; }
	}

	public class ScriptInput : IInput
	{
		private readonly string filePath;
		private StreamReader reader;

		public ScriptInput(string filePath)
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
				return reader != null && (CurrentInput = reader.ReadLine()) != null;
			}
			catch (IOException)
			{
				return false;
			}
		}

		public string CurrentInput { get; private set; }

		public string Name
		{
			get { return string.Format("script file \"{0}\"", Path.GetFileName(filePath)); }
		}
	}
}