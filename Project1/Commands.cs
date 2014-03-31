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
		/// Executes the command.
		/// </summary>
		void Execute();
	}

	public class InitCommand : ICommand
	{
		public string Name
		{
			get { return "init"; }
		}

		public string Usage
		{
			get { return "init"; }
		}

		public string Description
		{
			get { return "Restores the system to its initial state."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return true;
		}

		public void Execute()
		{
		}
	}

	public class QuitCommand : ICommand
	{
		public string Name
		{
			get { return "quit"; }
		}

		public string Usage
		{
			get { return "quit"; }
		}

		public string Description
		{
			get { return "Terminates the execution of the system."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return true;
		}

		public void Execute()
		{

		}
	}

	public class CreateCommand : ICommand
	{
		private string processName;
		private int priority;

		public string Name
		{
			get { return "cr"; }
		}

		public string Usage
		{
			get { return "cr <name> <priority>"; }
		}

		public string Description
		{
			get
			{
				return
					"Creates a new process <name> at the priority level <priority>. <name> is a single character. <priority> can be 1 or 2.";
			}
		}

		public bool LoadParams(string[] tokens)
		{
			return tokens.TryLoadProcessName(0, out processName)
			       && tokens.TryLoadPriority(1, out priority);
		}

		public void Execute()
		{

		}
	}

	public class DestroyCommand : ICommand
	{
		private string processName;

		public string Name
		{
			get { return "de"; }
		}

		public string Usage
		{
			get { return "de <name>"; }
		}

		public string Description
		{
			get { return "Destroys the process <name> and all its descendants."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return tokens.TryLoadProcessName(0, out processName);
		}

		public void Execute()
		{

		}
	}

	public class RequestCommand : ICommand
	{
		private string resourceName;
		private int count;

		public string Name
		{
			get { return "req"; }
		}

		public string Usage
		{
			get { return "req <name> <count>"; }
		}

		public string Description
		{
			get
			{
				return
					"Requests <count> number of units of the resource <name>. <name> is a single character. <count> is a positive integer.";
			}
		}

		public bool LoadParams(string[] tokens)
		{
			return tokens.TryLoadResourceName(0, out resourceName)
			       && tokens.TryLoadCount(1, out count);
		}

		public void Execute()
		{
		}
	}

	public class ReleaseCommand : ICommand
	{
		private string resourceName;
		private int count;

		public string Name
		{
			get { return "rel"; }
		}

		public string Usage
		{
			get { return "rel <name> <count>"; }
		}

		public string Description
		{
			get
			{
				return
					"Releases <count> number of units of the resource <name>. <name> is a single character. <count> is a positive integer.";
			}
		}

		public bool LoadParams(string[] tokens)
		{
			return tokens.TryLoadResourceName(0, out resourceName)
				   && tokens.TryLoadCount(1, out count);
		}

		public void Execute()
		{
		}
	}

	public class TimeoutCommand : ICommand
	{
		public string Name
		{
			get { return "to"; }
		}

		public string Usage
		{
			get { return "to"; }
		}

		public string Description
		{
			get { return "Triggers a scheduling timeout."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return true;
		}

		public void Execute()
		{
		}
	}

	public class RequestIOCommand : ICommand
	{
		public string Name
		{
			get { return "rio"; }
		}

		public string Usage
		{
			get { return "rio"; }
		}

		public string Description
		{
			get { return "Requests IO."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return true;
		}

		public void Execute()
		{

		}
	}

	public class CompleteIOCommand : ICommand
	{
		public string Name
		{
			get { return "ioc"; }
		}

		public string Usage
		{
			get { return "ioc"; }
		}

		public string Description
		{
			get { return "Completes IO."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return true;
		}

		public void Execute()
		{

		}
	}

	public class ShowProcessCommand : ICommand
	{
		private string processName;

		public string Name
		{
			get { return "show-proc"; }
		}

		public string Usage
		{
			get { return "show-proc <name>"; }
		}

		public string Description
		{
			get { return "Displays information about the process named <name>. <name> is a single character."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return tokens.TryLoadProcessName(0, out processName);
		}

		public void Execute()
		{
			throw new System.NotImplementedException();
		}
	}

	public class ShowResourceCommand : ICommand
	{
		private string resourceName;

		public string Name
		{
			get { return "show-res"; }
		}

		public string Usage
		{
			get { return "show-res <name>"; }
		}

		public string Description
		{
			get { return "Displays information about the resource named <name>."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return tokens.TryLoadResourceName(0, out resourceName);
		}

		public void Execute()
		{

		}
	}

	public class ListProcessesCommand : ICommand
	{
		public string Name
		{
			get { return "list-proc"; }
		}

		public string Usage
		{
			get { return "list-proc"; }
		}

		public string Description
		{
			get { return "Lists all processes and their statuses."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return true;
		}

		public void Execute()
		{

		}
	}

	public class ListResourcesCommand : ICommand
	{
		public string Name
		{
			get { return "list-res"; }
		}

		public string Usage
		{
			get { return "list-res"; }
		}

		public string Description
		{
			get { return "Lists all resources and their statuses."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return true;
		}

		public void Execute()
		{

		}
	}

	public class HelpCommand : ICommand
	{
		public string Name
		{
			get { return "help"; }
		}

		public string Usage
		{
			get { return "help"; }
		}

		public string Description
		{
			get { return "Describes available commands and their usages."; }
		}

		public bool LoadParams(string[] tokens)
		{
			return true;
		}

		public void Execute()
		{

		}
	}

	public static class ParamLoaders
	{
		public static bool TryLoadProcessName(this string[] tokens, int index, out string processName)
		{
			processName = null;
			if (tokens == null || tokens.Length < index)
				return false;
			if (new StringInfo(tokens[index]).LengthInTextElements > 1)
				return false;
			return !string.IsNullOrWhiteSpace(processName = tokens[index]);
		}

		public static bool TryLoadCount(this string[] tokens, int index, out int count)
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

		public static bool TryLoadResourceName(this string[] tokens, int index, out string resourceName)
		{
			resourceName = null;
			if (tokens == null || tokens.Length < index)
				return false;
			return !string.IsNullOrWhiteSpace(resourceName = tokens[index]);
		}

		public static bool TryLoadPriority(this string[] tokens, int index, out int priority)
		{
			priority = 0;
			if (tokens == null || tokens.Length < index)
				return false;
			if (!int.TryParse(tokens[index], NumberStyles.None, CultureInfo.InvariantCulture, out priority))
				return false;
			return priority == 1 || priority == 2;
		}
	}
}