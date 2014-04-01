using System;

namespace Project1
{
	public interface IDispatcher
	{
		void Dispatch(string input);
	}

	public class Dispatcher : IDispatcher
	{
		private readonly ICommandRegistry commandRegistry;
		private readonly IOutput output;
		private readonly IMessageBoard messageBoard;

		public Dispatcher(ICommandRegistry commandRegistry, IOutput output, IMessageBoard messageBoard)
		{
			this.commandRegistry = commandRegistry;
			this.output = output;
			this.messageBoard = messageBoard;
		}

		public void Dispatch(string input)
		{
			// Skip blank lines.
			if (string.IsNullOrWhiteSpace(input))
				return;
			// Split into tokens by whitespace.
			var tokens = input.Split();
			// Command name is first token.
			ICommand command;
			if (!commandRegistry.TryGetCommand(tokens[0], out command))
			{
				output.WriteLine("Unrecognized command \"{0}\".", tokens[0]);
				return;
			}
			// Feed args to command.
			var args = new string[tokens.Length - 1];
			Array.Copy(tokens, 1, args, 0, tokens.Length - 1);
			if (!command.LoadParams(args))
			{
				output.WriteLine(
					"Invalid args \"{0}\" for command \"{2}\". Expected usage is \"{2}\".",
					string.Join(" ", args),
					command.Name,
					command.Usage);
				return;
			}
			// Execute.
			messageBoard.Send(command);
		}
	}
}