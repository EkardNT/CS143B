﻿using System;

namespace Project1
{
	public enum Purpose
	{
		Error,
		Output,
		Success,
		Info
	}

	public interface IOutput
	{
		void Write(Purpose purpose, string text);
		void Write(Purpose purpose, string format, params object[] args);
		void WriteLine(Purpose purpose, string text);
		void WriteLine(Purpose purpose, string format, params object[] args);
		IDisposable Indent();
	}

	public abstract class OutputBase : IOutput
	{
		private class Indentation : IDisposable
		{
			private readonly OutputBase outer;
			private bool disposed;

			public Indentation(OutputBase outer)
			{
				this.outer = outer;
				outer.indentationLevel++;
			}

			public void Dispose()
			{
				if (!disposed)
				{
					outer.indentationLevel--;
					if (outer.indentationLevel < 0)
						throw new InvalidOperationException("Negative indentation level.");
					disposed = true;
				}
			}
		}

		private int indentationLevel;
		private bool startOfLine;

		protected OutputBase()
		{
			startOfLine = true;
		}

		protected abstract void DoWrite(Purpose purpose, string text);
		protected abstract void DoWrite(Purpose purpose, string format, params object[] args);
		protected abstract void DoWriteLine(Purpose purpose, string text);
		protected abstract void DoWriteLine(Purpose purpose, string format, params object[] args);

		public IDisposable Indent()
		{
			return new Indentation(this);
		}

		public void Write(Purpose purpose, string text)
		{
			DoWrite(purpose,
				startOfLine && indentationLevel > 0 ? string.Join("", new string('\t', indentationLevel), text) : text);
			startOfLine = false;
		}

		public void Write(Purpose purpose, string format, params object[] args)
		{
			DoWrite(purpose,
				startOfLine && indentationLevel > 0 ? string.Join("", new string('\t', indentationLevel), format) : format, args);
			startOfLine = false;
		}

		public void WriteLine(Purpose purpose, string text)
		{
			DoWriteLine(purpose,
				startOfLine && indentationLevel > 0 ? string.Join("", new string('\t', indentationLevel), text) : text);
			startOfLine = true;
		}

		public void WriteLine(Purpose purpose, string format, params object[] args)
		{
			DoWriteLine(purpose,
				startOfLine && indentationLevel > 0 ? string.Join("", new string('\t', indentationLevel), format) : format, args);
			startOfLine = true;
		}
	}

	public class ConsoleOutput : OutputBase
	{
		protected override void DoWrite(Purpose purpose, string text)
		{
			SetColor(purpose);
			Console.Write(text);
			Console.ForegroundColor = ConsoleColor.White;
		}

		protected override void DoWrite(Purpose purpose, string format, params object[] args)
		{
			SetColor(purpose);
			Console.Write(format, args);
			Console.ForegroundColor = ConsoleColor.White;
		}

		protected override void DoWriteLine(Purpose purpose, string text)
		{
			SetColor(purpose);
			Console.WriteLine(text);
			Console.ForegroundColor = ConsoleColor.White;
		}

		protected override void DoWriteLine(Purpose purpose, string format, params object[] args)
		{
			SetColor(purpose);
			Console.WriteLine(format, args);
			Console.ForegroundColor = ConsoleColor.White;
		}

		private void SetColor(Purpose purpose)
		{
			switch (purpose)
			{
				case Purpose.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case Purpose.Output:
					Console.ForegroundColor = ConsoleColor.White;
					break;
				case Purpose.Success:
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				case Purpose.Info:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				default:
					throw new ArgumentOutOfRangeException("purpose");
			}
		}
	}
}