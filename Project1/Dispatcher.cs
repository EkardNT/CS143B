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
				output.WriteLine(Purpose.Error, "Unrecognized command \"{0}\".", tokens[0]);
				return;
			}
			// Feed args to command.
			var context = new LoadParamsContext(tokens, 1, tokens.Length - 1);
			command.LoadParams(context);
			if (context.Failed)
			{
				output.WriteLine(
					Purpose.Error, 
					"Invalid args \"{0}\" for command \"{1}\"",
					input,
					command.Usage);
				using (output.Indent())
				{
					foreach (var error in context.Errors)
						output.WriteLine(Purpose.Error, "- {0}", error);
				}
				return;
			}
			// Execute.
			messageBoard.Send(command);
		}
	}
}