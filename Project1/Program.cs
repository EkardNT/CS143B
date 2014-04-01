﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Resources;
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
				: (IInputSourceFactory) new ConsoleInputSourceFactory());
			kernel.Bind<ICommandRegistry>().To<CommandRegistry>().InSingletonScope();
			kernel.Bind<IOutput>().To<ConsoleOutput>().InSingletonScope();
			kernel.Bind<IMessageBoard>().To<MessageBoard>().InSingletonScope();
			kernel.Bind<IDispatcher>().To<Dispatcher>().InSingletonScope();

			return kernel;
		}

		public const int PriorityCount = 3;

		private readonly IInputSourceFactory inputSourceFactory;
		private readonly IOutput output;
		private readonly IDispatcher dispatcher;
		// Indexed by process priority.
		private readonly Node<Process>[] readyQueue;
		// Mapped by resource name.
		private readonly Dictionary<string, Resource> resources;
		// Stop processing the current input source?
		private bool quitInputSource;
		// All processes mapped by name.
		private readonly Dictionary<string, Process> processes;

		public Program(
			IInputSourceFactory inputSourceFactory,
			IOutput output,
			IMessageBoard messageBoard,
			IDispatcher dispatcher)
		{
			this.inputSourceFactory = inputSourceFactory;
			this.output = output;
			this.dispatcher = dispatcher;

			messageBoard.Receive<InitCommand>(OnInit);
			messageBoard.Receive<QuitCommand>(OnQuit);
			messageBoard.Receive<CreateCommand>(OnCreate);
			messageBoard.Receive<DestroyCommand>(OnDestroy);
			messageBoard.Receive<RequestCommand>(OnRequest);
			messageBoard.Receive<ReleaseCommand>(OnRelease);
			messageBoard.Receive<TimeoutCommand>(OnTimeout);
			messageBoard.Receive<RequestIOCommand>(OnRequestIO);
			messageBoard.Receive<CompleteIOCommand>(OnCompleteIO);
			messageBoard.Receive<ShowProcessCommand>(OnShowProcess);
			messageBoard.Receive<ShowResourceCommand>(OnShowResource);
			messageBoard.Receive<ListProcessesCommand>(OnListProcesses);
			messageBoard.Receive<ListResourcesCommand>(OnListResources);

			// Create simulation state.
			readyQueue = new Node<Process>[PriorityCount];
			resources = new Dictionary<string, Resource>();
			processes = new Dictionary<string, Process>();

			// Initialize simulation to default state.
			messageBoard.Send(new InitCommand());
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

				output.WriteLine(Purpose.Output, "{0} is running.", GetReadyProcess().Name);
				while (!quitInputSource && inputSource.MoveNext())
				{
					dispatcher.Dispatch(inputSource.CurrentInput);
					DistributeResources();
					var running = GetReadyProcess();
					if (running != null)
					{
						running.Status = ProcessStatus.Running;
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

		private Process GetReadyProcess()
		{
			if (readyQueue[2] != null)
				return readyQueue[2].Data;
			if (readyQueue[1] != null)
				return readyQueue[1].Data;
			if (readyQueue[0] != null)
				return readyQueue[0].Data;
			return null;
		}

		private void DistributeResources()
		{
			foreach (var resource in resources.Values)
			{
				// Satisfy as many as possible in strict FIFO order.
				while (resource.WaitingList != null && resource.WaitingList.Data.Amount <= resource.Available)
				{
					var request = resource.WaitingList.Data;

					// Reserve resources.
					resource.Available -= request.Amount;
					request.Process.HeldResources[resource.Name] += request.Amount;

					// Move process from waiting list to ready list.
					Node<AccessRequest>.Remove(ref resource.WaitingList, resource.WaitingList);
					request.Process.WaitingNode = null;
					request.Process.WaitingResourceName = null;
					request.Process.ReadyNode = new Node<Process>(request.Process);
					Node<Process>.AddToBack(ref readyQueue[request.Process.Priority], request.Process.ReadyNode);

					request.Process.Status = ProcessStatus.Ready;
				}
			}
		}

		private void ReserveResource(string resourceName, int count)
		{
			var process = GetReadyProcess();
			if (process == null)
			{
				output.WriteLine(Purpose.Error, "No ready process in the system.");
				return;
			}

			Resource resource;
			// Check resource exists.
			if (!resources.TryGetValue(resourceName, out resource))
			{
				output.WriteLine(Purpose.Error, "No resource with name \"{0}\" exists in the system.", resourceName);
				return;
			}

			// Check that the request is actually possible.
			if (count > resource.Total)
			{
				output.WriteLine(
					Purpose.Error, 
					"Only {0} units of resource \"{1}\" exist.",
					resource.Total,
					resource.Name);
				return;
			}

			// If there are sufficient units available to immediately
			// satisfy request, the process doesn't need to wait.
			if (count <= resource.Available)
			{
				resource.Available -= count;
				process.HeldResources[resource.Name] += count;
			}
				// Otherwise it must wait for sufficient units to become available.
			else
			{
				Node<Process>.Remove(ref readyQueue[process.Priority], process.ReadyNode);
				process.ReadyNode = null;

				var waitingNode = new Node<AccessRequest>(new AccessRequest(process, count));
				Node<AccessRequest>.AddToBack(ref resource.WaitingList, waitingNode);

				process.Status = ProcessStatus.Waiting;
				process.WaitingResourceName = resource.Name;
				process.WaitingNode = waitingNode;
			}
		}

		private void ReleaseResource(string resourceName, int count)
		{
			var process = GetReadyProcess();
			if (process == null)
			{
				output.WriteLine(Purpose.Error, "No ready process in the system.");
				return;
			}

			Resource resource;
			// Check resource exists.
			if (!resources.TryGetValue(resourceName, out resource))
			{
				output.WriteLine(Purpose.Error, "No resource with name \"{0}\" exists in the system.", resourceName);
				return;
			}

			// Ensure the process is holding at least as many units as
			// the amount to be released.
			if (process.HeldResources[resource.Name] < count)
			{
				output.WriteLine(
					Purpose.Error, 
					"Process \"{0}\" is not holding {1} units of resource \"{2}\".",
					process.Name,
					count,
					resource.Name);
				return;
			}

			// Release.
			resource.Available += count;
			process.HeldResources[resource.Name] -= count;
		}

		private void OnInit(InitCommand command)
		{
			// Clear state.
			processes.Clear();
			readyQueue[0] = readyQueue[1] = readyQueue[2] = null;
			resources.Clear();
			resources["R1"] = new Resource("R1", 1);
			resources["R2"] = new Resource("R2", 2);
			resources["R3"] = new Resource("R3", 3);
			resources["R4"] = new Resource("R4", 4);
			resources["IO"] = new Resource("IO", 1);

			// Start the Init process.
			var init = new Process("Init", null, 0) {Status = ProcessStatus.Ready};
			foreach (var resource in resources.Values)
				init.HeldResources[resource.Name] = 0;
			processes.Add(init.Name, init);
			Node<Process>.AddToBack(ref readyQueue[0], new Node<Process>(init));
		}

		private void OnQuit(QuitCommand command)
		{
			quitInputSource = true;
		}

		private void OnCreate(CreateCommand command)
		{
			// Create new process as child of currently running process.
			var parent = GetReadyProcess();
			var child = new Process(command.ProcessName, parent, command.Priority);
			foreach (var resource in resources.Values)
				child.HeldResources[resource.Name] = 0;
			processes.Add(child.Name, child);

			// Add to end of parent's linked list of children.
			var childListNode = new Node<Process>(child);
			Node<Process>.AddToBack(ref parent.ChildList, childListNode);

			// Add to end of ready queue for proper priority.
			var readyQueueNode = new Node<Process>(child);
			Node<Process>.AddToBack(ref readyQueue[child.Priority], readyQueueNode);

			child.Status = readyQueue[child.Priority].Data == child
				? ProcessStatus.Running
				: ProcessStatus.Ready;
		}

		private void OnDestroy(DestroyCommand command)
		{
			Process proc;
			if (!processes.TryGetValue(command.ProcessName, out proc))
			{
				output.WriteLine(Purpose.Error, "No process with name \"{0}\" exists in the system.", command.ProcessName);
				return;
			}
			DestroyProcessTree(proc);
		}

		private void DestroyProcessTree(Process root)
		{
			// Destroy all children...
			Node<Process>.VisitAll(root.ChildList, DestroyProcessTree);

			// ... before destroying root.
			DestroyProcess(root);
		}

		private void DestroyProcess(Process process)
		{
			// Cancel pending resource requests.
			if (process.WaitingNode != null)
			{
				var resource = resources[process.WaitingResourceName];
				Node<AccessRequest>.Remove(ref resource.WaitingList, process.WaitingNode);
				process.WaitingNode = null;
				process.WaitingResourceName = null;
			}

			// Release all resources.
			foreach (var kvPair in process.HeldResources)
				resources[kvPair.Key].Available += kvPair.Value;
			process.HeldResources.Clear();

			// Remove from ready queue.
			if (process.ReadyNode != null)
			{
				Node<Process>.Remove(ref readyQueue[process.Priority], process.ReadyNode);
				process.ReadyNode = null;
			}

			// Remove from master process list.
			processes.Remove(process.Name);
		}

		private void OnRequest(RequestCommand command)
		{
			ReserveResource(command.ResourceName, command.Count);
		}

		private void OnRelease(ReleaseCommand command)
		{
			ReleaseResource(command.ResourceName, command.Count);
		}

		private void OnTimeout(TimeoutCommand command)
		{
			var processBeingPreempted = GetReadyProcess();
			if (processBeingPreempted == null)
			{
				output.WriteLine(Purpose.Error, "No ready process in the system.");
				return;
			}

			// Advance the ready queue.
			processBeingPreempted.Status = ProcessStatus.Ready;
			readyQueue[processBeingPreempted.Priority] = readyQueue[processBeingPreempted.Priority].Next;
		}

		private void OnRequestIO(RequestIOCommand command)
		{
			ReserveResource("IO", 1);
		}

		private void OnCompleteIO(CompleteIOCommand command)
		{
			ReleaseResource("IO", 1);
		}

		private void OnShowProcess(ShowProcessCommand command)
		{
			Process process;
			if (!processes.TryGetValue(command.ProcessName, out process))
			{
				output.WriteLine(Purpose.Error, "No process with name \"{0}\" exists in the system.", command.ProcessName);
				return;
			}

			output.WriteLine(Purpose.Info, "PROCESS: {0}", process.Name);
		}

		private void OnShowResource(ShowResourceCommand command)
		{
			Resource resource;
			if (!resources.TryGetValue(command.ResourceName, out resource))
			{
				output.WriteLine(Purpose.Error, "No resource with name \"{0}\" exists in the system.", command.ResourceName);
				return;
			}

			output.WriteLine(Purpose.Info, "RESOURCE: {0}", resource.Name);
		}

		private void OnListProcesses(ListProcessesCommand command)
		{
			const string Format = "{0,5}{1,5}{2,8}{3,8}{4,10}";
			output.WriteLine(Purpose.Info, "PROCESSES: {0} total", processes.Count);
			using (output.Indent())
			{
				output.WriteLine(Purpose.Info, Format, "PID", "Pri", "Status", "Parent", "Children");
				foreach (var process in processes.Values)
					output.WriteLine(
						Purpose.Info,
						Format,
						process.Name,
						process.Priority,
						process.Status,
						process.Parent == null ? "-" : process.Parent.Name,
						Node<Process>.Count(process.ChildList));
			}
		}

		private void OnListResources(ListResourcesCommand command)
		{
			const string Format = "{0,6}{1,7}{2,6}{3,9}";
			output.WriteLine(Purpose.Info, "RESOURCES: {0} total", resources.Count);
			using (output.Indent())
			{
				output.WriteLine(Purpose.Info, Format, "Name", "Total", "Free", "Waiting");
				foreach (var resource in resources.Values)
					output.WriteLine(
						Purpose.Info,
						Format,
						resource.Name,
						resource.Total,
						resource.Available,
						Node<AccessRequest>.Count(resource.WaitingList));
			}
		}
	}
}