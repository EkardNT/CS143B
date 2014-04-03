using System;
using System.Collections.Generic;
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
		/// user input tokens, reporting any detected errors.
		/// </summary>
		void LoadParams(LoadParamsContext context);
	}

	public class LoadParamsContext
	{
		private readonly string[] tokens;
		private readonly int offset, count;
		private readonly List<string> errors;

		public LoadParamsContext(string[] tokens, int offset, int count)
		{
			this.tokens = tokens;
			this.offset = offset;
			this.count = count;
			errors = new List<string>(1);
		}

		public bool Failed
		{
			get { return errors.Count > 0; }
		}

		public int TokenCount
		{
			get { return count; }
		}

		public IEnumerable<string> Errors
		{
			get { return errors; }
		}

		public string this[int tokenIndex]
		{
			get
			{
				if (tokenIndex < 0 || tokenIndex >= count)
					throw new IndexOutOfRangeException();
				return tokens[tokenIndex + offset];
			}
		}

		public void ReportError(string error)
		{
			errors.Add(error);
		}
	}

	public abstract class CommandBase : ICommand
	{
		public abstract string Name { get; }
		public abstract string Usage { get; }
		public abstract string Description { get; }
		public abstract void LoadParams(LoadParamsContext context);

		protected static void TryLoadProcessName(LoadParamsContext context, int index, out string processName)
		{
			processName = null;
			if (context.TokenCount <= index)
				context.ReportError("No argument provided for process name.");
			else if (string.IsNullOrWhiteSpace(context[index]))
				context.ReportError("Process name cannot be empty.");
			else if (new StringInfo(context[index]).LengthInTextElements > 1)
				context.ReportError("Process name can be at most 1 character.");
			else
				processName = context[index];
		}

		protected static void TryLoadCount(LoadParamsContext context, int index, out int count)
		{
			count = 0;
			if (context.TokenCount <= index)
				count = 1; // Default value if not provided instead of error.
			else if (!int.TryParse(context[index], NumberStyles.None, CultureInfo.InvariantCulture, out count))
				context.ReportError("Failed to parse count as an integer value.");
			else if (count < 1)
				context.ReportError("Count cannot be less than 1.");
		}

		protected static void TryLoadResourceName(LoadParamsContext context, int index, out string resourceName)
		{
			resourceName = null;
			if (context.TokenCount <= index)
				context.ReportError("No argument provided for resource name.");
			else if (string.IsNullOrWhiteSpace(context[index]))
				context.ReportError("Resource name cannot be empty.");
			else
				resourceName = context[index];
		}

		protected static void TryLoadPriority(LoadParamsContext context, int index, out int priority)
		{
			priority = 0;
			if (context.TokenCount <= index)
				context.ReportError("No argument provided for priority.");
			else if (!int.TryParse(context[index], NumberStyles.None, CultureInfo.InvariantCulture, out priority))
				context.ReportError("Failed to parse priority as an integer value.");
			else if (priority != 1 && priority != 2)
				context.ReportError("Invalid priority, must be 1 or 2.");
		}
	}

	public class InitCommand : CommandBase
	{
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

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}

	public class QuitCommand : CommandBase
	{
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

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}

	public class CreateCommand : CommandBase
	{
		private string processName;
		private int priority;

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

		public override void LoadParams(LoadParamsContext context)
		{
			TryLoadProcessName(context, 0, out processName);
			TryLoadPriority(context, 1, out priority);
		}
	}

	public class DestroyCommand : CommandBase
	{
		private string processName;

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

		public override void LoadParams(LoadParamsContext context)
		{
			TryLoadProcessName(context, 0, out processName);
		}
	}

	public class RequestCommand : CommandBase
	{
		private string resourceName;
		private int count;

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

		public override void LoadParams(LoadParamsContext context)
		{
			TryLoadResourceName(context, 0, out resourceName);
			TryLoadCount(context, 1, out count);
		}
	}

	public class ReleaseCommand : CommandBase
	{
		private string resourceName;
		private int count;

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

		public override void LoadParams(LoadParamsContext context)
		{
			TryLoadResourceName(context, 0, out resourceName);
			TryLoadCount(context, 1, out count);
		}
	}

	public class TimeoutCommand : CommandBase
	{
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

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}

	public class RequestIOCommand : CommandBase
	{
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

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}

	public class CompleteIOCommand : CommandBase
	{
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

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}

	public class ShowProcessCommand : CommandBase
	{
		private string processName;

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

		public override void LoadParams(LoadParamsContext context)
		{
			TryLoadProcessName(context, 0, out processName);
		}
	}

	public class ShowResourceCommand : CommandBase
	{
		private string resourceName;

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

		public override void LoadParams(LoadParamsContext context)
		{
			TryLoadResourceName(context, 0, out resourceName);
		}
	}

	public class ListProcessesCommand : CommandBase
	{
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

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}

	public class ListResourcesCommand : CommandBase
	{
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

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}

	public class HelpCommand : CommandBase
	{
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

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}

	public class DebugCommand : CommandBase
	{
		public override string Name
		{
			get { return "debug"; }
		}

		public override string Usage
		{
			get { return "debug"; }
		}

		public override string Description
		{
			get { return "Displays detailed simulator information for debugging."; }
		}

		public override void LoadParams(LoadParamsContext context)
		{
		}
	}
}