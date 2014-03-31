namespace Project1
{
	public class Resource
	{
		public Resource(string name, int available)
		{
			Name = name;
			Available = available;
		}

		public string Name { get; private set; }
		public int Available { get; set; }
		public Node<Process> WaitingList { get; set; }
	}

	public interface IResourceManager
	{
		void Reset();
	}

	public class ResourceManager : IResourceManager
	{
		public ResourceManager(IMessageBoard messageBoard)
		{
			messageBoard.Receive<RequestCommand>(OnRequestCommand);
			messageBoard.Receive<ReleaseCommand>(OnReleaseCommand);
		}

		public void Reset()
		{
			
		}

		private void OnRequestCommand(RequestCommand command)
		{
			
		}

		private void OnReleaseCommand(ReleaseCommand command)
		{
			
		}
	}
}