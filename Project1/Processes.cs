namespace Project1
{
	public class Process
	{
		public Process(string name, Process parent, int priority)
		{
			Name = name;
			Parent = parent;
			Priority = priority;
		}

		public string Name { get; private set; }
		public Process Parent { get; private set; }
		public int Priority { get; set; }
		public Node<Process> Children { get; set; }
	}

	public interface IProcessManager
	{
		void Reset();
	}

	public class ProcessManager : IProcessManager
	{
		public ProcessManager(IMessageBoard messageBoard)
		{
			messageBoard.Receive<CreateCommand>(OnCreateCommand);
			messageBoard.Receive<DestroyCommand>(OnDestroyCommand);
		}

		public void Reset()
		{

		}

		private void OnCreateCommand(CreateCommand command)
		{

		}

		private void OnDestroyCommand(DestroyCommand command)
		{

		}
	}
}