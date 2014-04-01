using System;
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
		private readonly IOutput output;

		public CommandRegistry(IMessageBoard messageBoard, IOutput output)
		{
			this.output = output;
			commands = new Dictionary<string, ICommand>();
			foreach (var command in typeof (CommandRegistry).Assembly.GetTypes()
				.Where(type => typeof (ICommand).IsAssignableFrom(type))
				.Where(type => !type.IsAbstract && !type.IsInterface)
				.Select(Activator.CreateInstance)
				.Cast<ICommand>())
			{
				commands.Add(command.Name, command);
			}

			messageBoard.Receive<HelpCommand>(OnHelp);
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

		private void OnHelp(HelpCommand command)
		{
			output.WriteLine(Purpose.Info, "{0,10} - {1}", "Name", "Description");
			output.WriteLine(Purpose.Info, new string('-', 80));
			foreach (var c in commands.Values)
			{
				output.WriteLine(Purpose.Info, "{0,10} - {1}", c.Name, c.Description);
				output.WriteLine(Purpose.Info, c.Usage);
			}
		}
	}
}