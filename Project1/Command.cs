namespace Project1
{
	public interface ICommand
	{
		string Name { get; }
		string Usage { get; }
		string Description { get; }
		bool ParseParameters(string[] parameters);
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

		public bool ParseParameters(string[] parameters)
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

		public bool ParseParameters(string[] parameters)
		{
			return true;
		}

		public void Execute()
		{

		}
	}

	public class CreateCommand : ICommand
	{
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

		public bool ParseParameters(string[] parameters)
		{
			return false;
		}

		public void Execute()
		{

		}
	}

	public class DestroyCommand : ICommand
	{
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

		public bool ParseParameters(string[] parameters)
		{
			return false;
		}

		public void Execute()
		{

		}
	}

	public class RequestCommand : ICommand
	{
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

		public bool ParseParameters(string[] parameters)
		{
			return false;
		}

		public void Execute()
		{
		}
	}

	public class ReleaseCommand : ICommand
	{
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

		public bool ParseParameters(string[] parameters)
		{
			return false;
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

		public bool ParseParameters(string[] parameters)
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

		public bool ParseParameters(string[] parameters)
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

		public bool ParseParameters(string[] parameters)
		{
			return true;
		}

		public void Execute()
		{

		}
	}

	public class ShowProcessCommand : ICommand
	{
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

		public bool ParseParameters(string[] parameters)
		{
			return false;
		}

		public void Execute()
		{
			throw new System.NotImplementedException();
		}
	}

	public class ShowResourceCommand : ICommand
	{
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

		public bool ParseParameters(string[] parameters)
		{
			return false;
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

		public bool ParseParameters(string[] parameters)
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

		public bool ParseParameters(string[] parameters)
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

		public bool ParseParameters(string[] parameters)
		{
			return true;
		}

		public void Execute()
		{

		}
	}
}