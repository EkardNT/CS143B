using System.Text;
namespace Project3
{
	public abstract class CommandBuilder
	{
		private const int MaxLogicalNameLength = 4;

		public CommandBuilder(string shellCommand, string usage, string description)
		{
			ShellCommand = shellCommand;
			Usage = usage;
			Description = description;
		}

		public string ShellCommand { get; private set; }
		public string Usage { get; private set; }
		public string Description { get; private set; }

		public abstract bool TryBuildCommand(string[] args, out object command);

		protected bool RequireLogicalName(string[] args, int argPos, out string logicalName)
		{
			logicalName = null;
			if (argPos >= args.Length)
				return false;
			if (Encoding.UTF8.GetByteCount(args[argPos]) > MaxLogicalNameLength)
				return false;
			logicalName = args[argPos];
			return true;
		}

		protected bool RequireFileHandle(string[] args, int argPos, out int fileHandle)
		{
			fileHandle = 0;
			if (argPos >= args.Length)
				return false;
			return int.TryParse(args[argPos], out fileHandle);
		}

		protected bool RequireCount(string[] args, int argPos, out int count)
		{
			count = 0;
			if (argPos >= args.Length)
				return false;
			return int.TryParse(args[argPos], out count);
		}

		protected bool RequirePosition(string[] args, int argPos, out int position)
		{
			position = 0;
			if (argPos >= args.Length)
				return false;
			return int.TryParse(args[argPos], out position);
		}

		protected bool RequireCharValue(string[] args, int argPos, out byte value)
		{
			value = 0;
			if (argPos >= args.Length)
				return false;
			var bytes = Encoding.UTF8.GetBytes(args[argPos]);
			if (bytes.Length == 1)
			{
				value = bytes[0];
				return true;
			}
			return false;
		}

		protected bool RequirePath(string[] args, int argPos, out string path)
		{
			path = null;
			if (argPos >= args.Length)
				return false;
			return !string.IsNullOrWhiteSpace(path = args[argPos]);
		}

		protected void AcceptPath(string[] args, int argPos, out string path)
		{
			path = null;
			if(argPos >= args.Length)
				return;
			path = args[argPos];
		}
	}

	public class CreateCommandBuilder : CommandBuilder
	{
		public CreateCommandBuilder() : base("cd", "cd <name>", "Creates a new file named <name>.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var cr = (command = new CreateCommand()) as CreateCommand;
			return RequireLogicalName(args, 0, out cr.LogicalName);
		}
	}

	public class DestroyCommandBuilder : CommandBuilder
	{
		public DestroyCommandBuilder() : base("de", "de <name>", "Destroys the file named <name>.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var de = (command = new DestroyCommand()) as DestroyCommand;
			return RequireLogicalName(args, 0, out de.LogicalName);
		}
	}

	public class OpenCommandBuilder : CommandBuilder
	{
		public OpenCommandBuilder() : base("op", "op <name>", "Opens the file named <name> for reading and writing; outputs the file handle.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var op = (command = new OpenCommand()) as OpenCommand;
			return RequireLogicalName(args, 0, out op.LogicalName);
		}
	}

	public class CloseCommandBuilder : CommandBuilder
	{
		public CloseCommandBuilder() : base("cl", "cl <handle>", "Closes the file with handle <handle>.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var cl = (command = new CloseCommand()) as CloseCommand;
			return RequireFileHandle(args, 0, out cl.FileHandle);
		}
	}

	public class ReadCommandBuilder : CommandBuilder
	{
		public ReadCommandBuilder() : base("rd", "rd <handle> <count>", "Reads <count> sequential bytes from the file with handle <handle>.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var rd = (command = new ReadCommand()) as ReadCommand;
			return RequireFileHandle(args, 0, out rd.FileHandle)
				&& RequireCount(args, 1, out rd.Count);
		}
	}

	public class WriteCommandBuilder : CommandBuilder
	{
		public WriteCommandBuilder() : base("wr", "wr <index> <char> <count>", "Writes value of <char> character into the file with handle <handle> <count> times.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var wr = (command = new WriteCommand()) as WriteCommand;
			return RequireFileHandle(args, 0, out wr.FileHandle)
				&& RequireCharValue(args, 1, out wr.Data)
				&& RequireCount(args, 2, out wr.Count);
		}
	}

	public class SeekCommandBuilder : CommandBuilder
	{
		public SeekCommandBuilder() : base("sk", "sk <index> <pos>", "Set the data pointer of the file with handle <handle> to <pos>.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var sk = (command = new SeekCommand()) as SeekCommand;
			return RequireFileHandle(args, 0, out sk.FileHandle)
				&& RequirePosition(args, 1, out sk.Position);
		}
	}

	public class DirectoryCommandBuilder : CommandBuilder
	{
		public DirectoryCommandBuilder() : base("dr", "dr", "Lists the names of all files.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			command = new DirectoryCommand();
			return true;
		}
	}

	public class InitCommandBuilder : CommandBuilder
	{
		public InitCommandBuilder() : base("in", "in <path>", "Initializes the file system to an empty state, or restores state from a serialization file if <path> is provided.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var @in = (command = new InitCommand()) as InitCommand;
			AcceptPath(args, 0, out @in.SerializationFilePath);
			return true;
		}
	}

	public class SaveCommandBuilder : CommandBuilder
	{
		public SaveCommandBuilder() : base("sv", "sv <path", "Closes all files and saves the state of the file system to the (real) file at <path>.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			var sv = (command = new SaveCommand()) as SaveCommand;
			return RequirePath(args, 0, out sv.SerializationFilePath);
		}
	}

	public class HelpCommandBuilder : CommandBuilder
	{
		public HelpCommandBuilder() : base("help", "help", "Lists all available commands and their descriptions.") { }

		public override bool TryBuildCommand(string[] args, out object command)
		{
			command = new HelpCommand();
			return true;
		}
	}
}