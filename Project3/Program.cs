using System;

namespace Project3
{
	public static class Program
	{
		private static BuilderRegistry builders;
		private static FileSystem fileSystem;
		private static bool quit;

		public static void Main()
		{
			builders = new BuilderRegistry();
			fileSystem = new FileSystem();
			var messageBoard = PrepareMessageBoard();

			quit = false;

			Console.ForegroundColor = ConsoleColor.White;

			// Main loop.
			while (!quit)
			{
				// Split input on whitespace.
				var tokens = Console.ReadLine().Split();
				if (tokens.Length == 0)
					continue;

				// Retrieve command builder.
				CommandBuilder builder;
				if (!builders.TryGetBuilder(tokens[0], out builder))
				{
					WriteErrorLine("unknown command.");
					continue;
				}

				// Prepare arguments array.
				var arguments = new string[tokens.Length - 1];
				for (int i = 0; i < arguments.Length; i++)
					arguments[i] = tokens[i + 1];

				// Build command.
				object command;
				if (!builder.TryBuildCommand(arguments, out command))
				{
					WriteErrorLine("invalid arguments.");
					continue;
				}

				// Dispatch.
				try
				{
					messageBoard.Send(command);
				}
				catch (FileSystemException e)
				{
					WriteErrorLine(e.Message);
				}
				catch (NotImplementedException e)
				{
					WriteErrorLine("not implemented");
				}
			}
		}

		private static MessageBoard PrepareMessageBoard()
		{
			var mb = new MessageBoard();
			mb.Receive<CreateCommand>(On);
			mb.Receive<DestroyCommand>(On);
			mb.Receive<OpenCommand>(On);
			mb.Receive<CloseCommand>(On);
			mb.Receive<WriteCommand>(On);
			mb.Receive<ReadCommand>(On);
			mb.Receive<SeekCommand>(On);
			mb.Receive<DirectoryCommand>(On);
			mb.Receive<InitCommand>(On);
			mb.Receive<SaveCommand>(On);
			mb.Receive<HelpCommand>(On);
			mb.Receive<ExitCommand>(On);
			return mb;
		}

		private static void On(CreateCommand command)
		{
			fileSystem.Create(command.LogicalName);
			WriteSuccessLine(string.Format("file {0} created", command.LogicalName));
		}

		private static void On(DestroyCommand command)
		{
			fileSystem.Destroy(command.LogicalName);
			WriteSuccessLine(string.Format("file {0} destroyed", command.LogicalName));
		}

		private static void On(OpenCommand command)
		{
			int handle = fileSystem.Open(command.LogicalName);
			WriteSuccessLine(string.Format("file {0} opened, index={1}", command.LogicalName, handle));
		}

		private static void On(CloseCommand command)
		{
			fileSystem.Close(command.FileHandle);
			WriteSuccessLine(string.Format("file {0} closed", command.FileHandle));
		}

		private static void On(ReadCommand command)
		{
			var bytes = new byte[command.Count];
			fileSystem.Read(command.FileHandle, bytes, command.Count);
			WriteSuccess(string.Format("{0} bytes read: ", command.Count));
			for (int i = 0; i < command.Count; i++)
				WriteSuccess(((char)bytes[i]).ToString());
			WriteSuccessLine("");
		}

		private static void On(WriteCommand command)
		{
			var bytes = new byte[command.Count];
			for (int i = 0; i < command.Count; i++)
				bytes[i] = command.Data;
			fileSystem.Write(command.FileHandle, bytes, command.Count);
			WriteSuccessLine(string.Format("{0} bytes written", command.Count));
		}

		private static void On(SeekCommand command)
		{
			fileSystem.Seek(command.FileHandle, command.Position);
			WriteSuccessLine(string.Format("current position is {0}", command.Position));
		}

		private static void On(DirectoryCommand command)
		{
			foreach (var file in fileSystem.Directory())
				WriteSuccessLine(file);
		}

		private static void On(InitCommand command)
		{
			fileSystem.Init(command.SerializationFilePath);
			WriteSuccessLine(command.SerializationFilePath == null
				? "disk initialized"
				: "disk restored");
		}

		private static void On(SaveCommand command)
		{
			fileSystem.Save(command.SerializationFilePath);
			WriteSuccessLine("disk saved");
		}

		private static void On(HelpCommand command)
		{
			foreach(var builder in builders)
			{
				WriteHelp("- ");
				WriteHelpLine(builder.Usage);
				WriteHelp("\t");
				WriteHelpLine(builder.Description);
			}
		}

		private static void On(ExitCommand command)
		{
			quit = true;
		}

		private static void WriteErrorLine(string error)
		{
			Console.ForegroundColor = ConsoleColor.Red;
#if DEBUG
			Console.WriteLine("error: " + error);
#else
			Console.WriteLine("error");
#endif
			Console.ForegroundColor = ConsoleColor.White;
		}

		private static void WriteHelp(string text)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(text);
			Console.ForegroundColor = ConsoleColor.White;
		}

		private static void WriteHelpLine(string text)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(text);
			Console.ForegroundColor = ConsoleColor.White;
		}

		private static void WriteSuccessLine(string text)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(text);
			Console.ForegroundColor = ConsoleColor.White;
		}

		private static void WriteSuccess(string text)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(text);
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
