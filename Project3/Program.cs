﻿using System;
using System.IO;

namespace Project3
{
	public static class Program
	{
		private static BuilderRegistry builders;
		private static FileSystem fileSystem;
		private static IOutput output;
		private static bool quit;

		public static void Main()
		{
			var messageBoard = PrepareMessageBoard();
			builders = new BuilderRegistry();
			fileSystem = new FileSystem();
			quit = false;

			IInput input;
			PrepareIO(out input, out output);

			try
			{
				input.Init();
				output.Init();
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception occurred while initializing input/output: {0}", e.Message);
				return;
			}

			// Main loop.
			while (!quit && input.MoveNext())
			{
				// Split input on whitespace.
				var tokens = input.CurrentInput.Split();
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
				catch (NotImplementedException)
				{
					WriteErrorLine("not implemented");
				}
			}

			output.Dispose();
			input.Dispose();
		}

		private static void PrepareIO(out IInput input, out IOutput output)
		{
			// If input.txt exists, read from there and output to my_student_id.txt.
			if (File.Exists("input.txt"))
			{
				input = new ScriptInput("input.txt");
				output = new TextFileOutput("35571095.txt");
			}
			// Otherwise read from and write to console.
			else
			{
				input = new ConsoleInput();
				output = new ConsoleOutput();
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
			if (command.SerializationFilePath == null)
			{
				fileSystem.Init();
				WriteSuccessLine("disk initialized");
			}
			else
			{
				fileSystem.Load(command.SerializationFilePath);
				WriteSuccessLine("disk restored");
			}
		}

		private static void On(SaveCommand command)
		{
			fileSystem.Save(command.SerializationFilePath);
			WriteSuccessLine("disk saved");
		}

		private static void On(HelpCommand command)
		{
			foreach (var builder in builders)
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
			output.WriteLine(Purpose.Error, error);
#else
			output.WriteLine(Purpose.Error, "error");
#endif
			Console.ForegroundColor = ConsoleColor.White;
		}

		private static void WriteHelp(string text)
		{
			output.Write(Purpose.Info, text);
		}

		private static void WriteHelpLine(string text)
		{
			output.WriteLine(Purpose.Info, text);
		}

		private static void WriteSuccessLine(string text)
		{
			output.WriteLine(Purpose.Success, text);
		}

		private static void WriteSuccess(string text)
		{
			output.Write(Purpose.Success, text);
		}
	}
}