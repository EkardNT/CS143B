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

			kernel.Bind<IInputSourceFactory>().ToConstant(args.Length > 0
				? (IInputSourceFactory)new ScriptInputSourceFactory(args)
				: (IInputSourceFactory)new ConsoleInputSourceFactory());
			kernel.Bind<IActivator>().ToConstant(new NinjectActivator(kernel)).InSingletonScope();
			kernel.Bind<ICommandRegistry>().To<CommandRegistry>().InSingletonScope();
			kernel.Bind<IOutput>().To<ConsoleOutput>().InSingletonScope();

			return kernel;
		}

		[Inject]
		public IInputSourceFactory InputSourceFactory { get; set; }

		public void Run()
		{
			foreach (var inputSource in InputSourceFactory.Sources)
			{
				Console.Write("Attempting to process input from the {0}... ", inputSource.Name);

				try
				{
					inputSource.Init();
					Console.WriteLine("successfully initialized.");
				}
				catch
				{
					Console.WriteLine("failed, skipping this input source.");
				}

				while (inputSource.MoveNext())
				{
					
				}
			}
		}
	}
}