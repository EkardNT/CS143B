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

			kernel.Bind<IInputSourceFactory>().ToConstant(args.Length > 0
				? (IInputSourceFactory) new ScriptInputSourceFactory(args)
				: new ConsoleInputSourceFactory());
			kernel.Bind<ICommandRegistry>().To<CommandRegistry>().InSingletonScope();
			kernel.Bind<IOutput>().To<ConsoleOutput>().InSingletonScope();
			kernel.Bind<IMessageBoard>().To<MessageBoard>().InSingletonScope();
			kernel.Bind<IDispatcher>().To<Dispatcher>().InSingletonScope();
			kernel.Bind<ISimulator>().To<Simulator>().InSingletonScope();

			return kernel;
		}

		private readonly IInputSourceFactory inputSourceFactory;
		private readonly IOutput output;
		private readonly IDispatcher dispatcher;
		private readonly ISimulator simulator;

		// Stop processing the current input source?
		private bool quitInputSource;

		public Program(
			IInputSourceFactory inputSourceFactory,
			IOutput output,
			IMessageBoard messageBoard,
			IDispatcher dispatcher,
			ISimulator simulator)
		{
			this.inputSourceFactory = inputSourceFactory;
			this.output = output;
			this.dispatcher = dispatcher;
			this.simulator = simulator;

			messageBoard.Receive<QuitCommand>(OnQuit);
		}

		public void Run()
		{
			foreach (var inputSource in inputSourceFactory.Sources)
			{
				output.Write(Purpose.Output, "Attempting to process input from the {0}... ", inputSource.Name);

				try
				{
					inputSource.Init();
					output.WriteLine(Purpose.Success, "successfully initialized.");
				}
				catch
				{
					output.WriteLine(Purpose.Error, "failed, skipping this input source.");
					continue;
				}

				output.WriteLine(Purpose.Output, "{0} is running.", simulator.RunningProcess.Name);
				while (!quitInputSource && inputSource.MoveNext())
				{
					dispatcher.Dispatch(inputSource.CurrentInput);
					simulator.Tick();
					var running = simulator.RunningProcess;
					if (running != null)
					{
						output.WriteLine(Purpose.Output, "{0} is running.", running.Name);
					}
					else
						output.WriteLine(Purpose.Output, "No processes in system.");
				}

				output.WriteLine(
					Purpose.Output,
					quitInputSource
						? "Quit processing input from the {0}."
						: "Finished processing input from the {0}.",
					inputSource.Name);
			}
		}

		private void OnQuit(QuitCommand command)
		{
			quitInputSource = true;
		}
	}
}