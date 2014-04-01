using System;

namespace Project1
{
	public interface IOutput
	{
		void Write(string text);
		void Write(string format, params object[] args);
		void WriteLine(string text);
		void WriteLine(string format, params object[] args);
	}

	public class ConsoleOutput : IOutput
	{
		public void Write(string text)
		{
			Console.Write(text);
		}

		public void Write(string format, params object[] args)
		{
			Console.Write(format, args);
		}

		public void WriteLine(string text)
		{
			Console.WriteLine(text);
		}

		public void WriteLine(string format, params object[] args)
		{
			Console.WriteLine(format, args);
		}
	}
}