using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Project1
{
	public interface ICommandRegistry : IEnumerable<ICommand>
	{
		bool TryGetCommand(string commandName, out ICommand command);
	}

	public class CommandRegistry : ICommandRegistry
	{
		private readonly Dictionary<string, ICommand> commands;

		public CommandRegistry(IActivator activator)
		{
			commands = new Dictionary<string, ICommand>();
			foreach (var command in typeof (CommandRegistry).Assembly.DefinedTypes
				.Where(type => typeof (ICommand).IsAssignableFrom(type))
				.Where(type => !type.IsAbstract && !type.IsInterface)
				.Select(activator.Create)
				.Cast<ICommand>())
			{
				commands.Add(command.Name, command);
			}
		}

		public bool TryGetCommand(string commandName, out ICommand command)
		{
			return commands.TryGetValue(commandName, out command);
		}

		public IEnumerator<ICommand> GetEnumerator()
		{
			return commands.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}