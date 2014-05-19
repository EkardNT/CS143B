using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
	public static class Program
	{
		private static BuilderRegistry builders;
		private static FileSystem fileSystem;
		private static bool quit;

		public static void Main(string[] args)
		{
			builders = new BuilderRegistry();
			fileSystem = new FileSystem(4096, 1024);
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
					WriteErrorLine("Unknown command.");
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
					WriteErrorLine("Invalid arguments.");
					continue;
				}

				// Dispatch.
				messageBoard.Send(command);
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
			Console.WriteLine("Create");
		}

		private static void On(DestroyCommand command)
		{
			Console.WriteLine("Destroy");
		}

		private static void On(OpenCommand command)
		{
			Console.WriteLine("Open");
		}

		private static void On(CloseCommand command)
		{
			Console.WriteLine("Close");
		}

		private static void On(ReadCommand command)
		{
			Console.WriteLine("Read");
		}

		private static void On(WriteCommand command)
		{
			Console.WriteLine("Write");
		}

		private static void On(SeekCommand command)
		{
			Console.WriteLine("Seek");
		}

		private static void On(DirectoryCommand command)
		{
			Console.WriteLine("Directory");
		}

		private static void On(InitCommand command)
		{
			Console.WriteLine("Init");
		}

		private static void On(SaveCommand command)
		{
			Console.WriteLine("Save");
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

		private static void WriteErrorLine(string text)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(text);
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
	}
}
