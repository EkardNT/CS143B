using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1
{
	public interface IInputSourceFactory : IDisposable
	{
		IEnumerable<IInputSource> Sources { get; }
	}

	public class ConsoleInputSourceFactory : IInputSourceFactory
	{
		public ConsoleInputSourceFactory()
		{
			Sources = new[] {new ConsoleInputSource()};
		}

		public IEnumerable<IInputSource> Sources { get; private set; }

		public void Dispose()
		{

		}
	}

	public class ScriptInputSourceFactory : IInputSourceFactory
	{
		public ScriptInputSourceFactory(IEnumerable<string> filePaths)
		{
			Sources = filePaths.Select(path => new ScriptInputSource(path));
		}

		public IEnumerable<IInputSource> Sources { get; private set; }

		public void Dispose()
		{
			foreach (var source in Sources)
				source.Dispose();
		}
	}
}