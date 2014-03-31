using System.Globalization;

namespace Project1
{
	public interface ICommand
	{
		/// <summary>
		/// The name the user must input to run the command.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Explanatory usage, of the form "command-name &lt;param1&gt; &lt;param2&gt;...
		/// </summary>
		string Usage { get; }

		/// <summary>
		/// What the command does.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Attempts to retrieve any required parameter data from the provided
		/// user input tokens. Returns true if the required parameters were
		/// successfully retrieved, false otherwise.
		/// </summary>
		bool LoadParams(string[] tokens);

		/// <summary>
		/// Sends a message into the system message board announcing the command.
		/// </summary>
		void DispatchMessage();
	}

	public abstract class CommandBase : ICommand
	{
		protected CommandBase(IMessageBoard messageBoard)
		{
			MessageBoard = messageBoard;
		}

		protected IMessageBoard MessageBoard { get; private set; }

		public abstract string Name { get; }
		public abstract string Usage { get; }
		public abstract string Description { get; }
		public abstract bool LoadParams(string[] tokens);
		public abstract void DispatchMessage();

		protected static bool TryLoadProcessName(string[] tokens, int index, out string processName)
		{
			processName = null;
			if (tokens == null || tokens.Length < index)
				return false;
			if (new StringInfo(tokens[index]).LengthInTextElements > 1)
				return false;
			return !string.IsNullOrWhiteSpace(processName = tokens[index]);
		}

		protected static bool TryLoadCount(string[] tokens, int index, out int count)
		{
			count = 0;
			if (tokens == null || tokens.Length < index)
				return false;
			if (!int.TryParse(tokens[index], NumberStyles.None, CultureInfo.InvariantCulture, out count))
				return false;
			if (count < 1)
				return false;
			return true;
		}

		protected static bool TryLoadResourceName(string[] tokens, int index, out string resourceName)
		{
			resourceName = null;
			if (tokens == null || tokens.Length < index)
				return false;
			return !string.IsNullOrWhiteSpace(resourceName = tokens[index]);
		}

		protected static bool TryLoadPriority(string[] tokens, int index, out int priority)
		{
			priority = 0;
			if (tokens == null || tokens.Length < index)
				return false;
			if (!int.TryParse(tokens[index], NumberStyles.None, CultureInfo.InvariantCulture, out priority))
				return false;
			return priority == 1 || priority == 2;
		}
	}

	public class InitCommand : CommandBase
	{
		public InitCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public override string Name
		{
			get { return "init"; }
		}

		public override string Usage
		{
			get { return "init"; }
		}

		public override string Description
		{
			get { return "Restores the system to its initial state."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return true;
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class QuitCommand : CommandBase
	{
		public QuitCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public override string Name
		{
			get { return "quit"; }
		}

		public override string Usage
		{
			get { return "quit"; }
		}

		public override string Description
		{
			get { return "Terminates the execution of the system."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return true;
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class CreateCommand : CommandBase
	{
		private string processName;
		private int priority;

		public CreateCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public string ProcessName
		{
			get { return processName; }
		}

		public int Priority
		{
			get { return priority; }
		}

		public override string Name
		{
			get { return "cr"; }
		}

		public override string Usage
		{
			get { return "cr <name> <priority>"; }
		}

		public override string Description
		{
			get
			{
				return
					"Creates a new process <name> at the priority level <priority>. <name> is a single character. <priority> can be 1 or 2.";
			}
		}

		public override bool LoadParams(string[] tokens)
		{
			return TryLoadProcessName(tokens, 0, out processName)
			       && TryLoadPriority(tokens, 1, out priority);
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class DestroyCommand : CommandBase
	{
		private string processName;

		public DestroyCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public string ProcessName
		{
			get { return processName; }
		}

		public override string Name
		{
			get { return "de"; }
		}

		public override string Usage
		{
			get { return "de <name>"; }
		}

		public override string Description
		{
			get { return "Destroys the process <name> and all its descendants."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return TryLoadProcessName(tokens, 0, out processName);
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class RequestCommand : CommandBase
	{
		private string resourceName;
		private int count;

		public RequestCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public string ResourceName
		{
			get { return resourceName; }
		}

		public int Count
		{
			get { return count; }
		}

		public override string Name
		{
			get { return "req"; }
		}

		public override string Usage
		{
			get { return "req <name> <count>"; }
		}

		public override string Description
		{
			get
			{
				return
					"Requests <count> number of units of the resource <name>. <name> is a single character. <count> is a positive integer.";
			}
		}

		public override bool LoadParams(string[] tokens)
		{
			return TryLoadResourceName(tokens, 0, out resourceName)
			       && TryLoadCount(tokens, 1, out count);
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class ReleaseCommand : CommandBase
	{
		private string resourceName;
		private int count;

		public ReleaseCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public string ResourceName
		{
			get { return resourceName; }
		}

		public int Count
		{
			get { return count; }
		}

		public override string Name
		{
			get { return "rel"; }
		}

		public override string Usage
		{
			get { return "rel <name> <count>"; }
		}

		public override string Description
		{
			get
			{
				return
					"Releases <count> number of units of the resource <name>. <name> is a single character. <count> is a positive integer.";
			}
		}

		public override bool LoadParams(string[] tokens)
		{
			return TryLoadResourceName(tokens, 0, out resourceName)
			       && TryLoadCount(tokens, 1, out count);
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class TimeoutCommand : CommandBase
	{
		public TimeoutCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public override string Name
		{
			get { return "to"; }
		}

		public override string Usage
		{
			get { return "to"; }
		}

		public override string Description
		{
			get { return "Triggers a scheduling timeout."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return true;
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class RequestIOCommand : CommandBase
	{
		public RequestIOCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public override string Name
		{
			get { return "rio"; }
		}

		public override string Usage
		{
			get { return "rio"; }
		}

		public override string Description
		{
			get { return "Requests IO."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return true;
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class CompleteIOCommand : CommandBase
	{
		public CompleteIOCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public override string Name
		{
			get { return "ioc"; }
		}

		public override string Usage
		{
			get { return "ioc"; }
		}

		public override string Description
		{
			get { return "Completes IO."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return true;
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class ShowProcessCommand : CommandBase
	{
		private string processName;

		public ShowProcessCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public string ProcessName
		{
			get { return processName; }
		}

		public override string Name
		{
			get { return "show-proc"; }
		}

		public override string Usage
		{
			get { return "show-proc <name>"; }
		}

		public override string Description
		{
			get { return "Displays information about the process named <name>. <name> is a single character."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return TryLoadProcessName(tokens, 0, out processName);
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class ShowResourceCommand : CommandBase
	{
		private string resourceName;

		public ShowResourceCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public string ResourceName
		{
			get { return resourceName; }
		}

		public override string Name
		{
			get { return "show-res"; }
		}

		public override string Usage
		{
			get { return "show-res <name>"; }
		}

		public override string Description
		{
			get { return "Displays information about the resource named <name>."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return TryLoadResourceName(tokens, 0, out resourceName);
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class ListProcessesCommand : CommandBase
	{
		public ListProcessesCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public override string Name
		{
			get { return "list-proc"; }
		}

		public override string Usage
		{
			get { return "list-proc"; }
		}

		public override string Description
		{
			get { return "Lists all processes and their statuses."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return true;
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class ListResourcesCommand : CommandBase
	{
		public ListResourcesCommand(IMessageBoard messageBoard) : base(messageBoard)
		{
		}

		public override string Name
		{
			get { return "list-res"; }
		}

		public override string Usage
		{
			get { return "list-res"; }
		}

		public override string Description
		{
			get { return "Lists all resources and their statuses."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return true;
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}

	public class HelpCommand : CommandBase
	{
		public HelpCommand(IMessageBoard messageBoard) : base(messageBoard)
		{

		}

		public override string Name
		{
			get { return "help"; }
		}

		public override string Usage
		{
			get { return "help"; }
		}

		public override string Description
		{
			get { return "Describes available commands and their usages."; }
		}

		public override bool LoadParams(string[] tokens)
		{
			return true;
		}

		public override void DispatchMessage()
		{
			MessageBoard.Send(this);
		}
	}
}