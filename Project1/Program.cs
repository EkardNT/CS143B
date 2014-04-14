using System;
using Ninject;

namespace Project1
{
	public class Program
	{
		public static void Main(string[] args)
		{
			using (var kernel = CompositionRoot(args))
			{
				var program = kernel.Get<Program>();
				program.Run();
			}
		}

		private static IKernel CompositionRoot(string[] args)
		{
			var kernel = new StandardKernel();

			ParseCommandLine(args, kernel);

			kernel.Bind<ICommandRegistry>().To<CommandRegistry>().InSingletonScope();
			kernel.Bind<IMessageBoard>().To<MessageBoard>().InSingletonScope();
			kernel.Bind<IDispatcher>().To<Dispatcher>().InSingletonScope();
			kernel.Bind<ISimulator>().To<Simulator>().InSingletonScope();

			return kernel;
		}

		private static void ParseCommandLine(string[] args, IKernel kernel)
		{
			// First argument is input file path, otherwise use console input.
			if (args.Length > 0)
				kernel.Bind<IInput>().ToConstant(new ScriptInput(args[0])).InSingletonScope();
			else
				kernel.Bind<IInput>().To<ConsoleInput>().InSingletonScope();

			// Second argument is output file path, otherwise use console output.
			if (args.Length > 1)
				kernel.Bind<IOutput>().ToConstant(new TextFileOutput(args[1])).InSingletonScope();
			else
				kernel.Bind<IOutput>().To<ConsoleOutput>().InSingletonScope();
		}

		private readonly IInput input;
		private readonly IOutput output;
		private readonly IDispatcher dispatcher;
		private readonly ISimulator simulator;

		// Early quit?
		private bool quit;

		public Program(
			IInput input,
			IOutput output,
			IMessageBoard messageBoard,
			IDispatcher dispatcher,
			ISimulator simulator)
		{
			this.input = input;
			this.output = output;
			this.dispatcher = dispatcher;
			this.simulator = simulator;

			messageBoard.Receive<QuitCommand>(OnQuit);
		}

		public void Run()
		{
			if (!InitIO())
				return;

			output.WriteLine(Purpose.Output, "{0} is running.", simulator.RunningProcess.Name);
			while (!quit && input.MoveNext())
			{
				// Emit blank output for blank input.
				if (string.IsNullOrWhiteSpace(input.CurrentInput))
				{
					output.WriteLine(Purpose.Output, "");
					continue;
				}
				// Update simulation.
				dispatcher.Dispatch(input.CurrentInput);
				simulator.Tick();
				// Report running process.
				if (!quit)
				{
					var running = simulator.RunningProcess;
					if (running != null)
						output.WriteLine(Purpose.Output, "{0} is running.", running.Name);
					else
						output.WriteLine(Purpose.Output, "No processes in system.");
				}
			}

			output.WriteLine(Purpose.Output, quit ? "Simulator terminated." : "Simulator finished.");
		}

		private bool InitIO()
		{
			var console = new ConsoleOutput();

			try
			{
				console.Write(Purpose.Info, "Initializing output... ");
				output.Init();
				console.WriteLine(Purpose.Success, "succeeded.");
			}
			catch (Exception e)
			{
				console.WriteLine(Purpose.Error, "failed, exiting. [{0}]", e.Message);
				return false;
			}

			try
			{
				console.Write(Purpose.Info, "Initializing input... ");
				input.Init();
				console.WriteLine(Purpose.Success, "succeeded.");
			}
			catch (Exception e)
			{
				console.WriteLine(Purpose.Error, "failed, exiting. [{0}]", e.Message);
				return false;
			}

			return true;
		}

		private void OnQuit(QuitCommand command)
		{
			quit = true;
		}
	}
}