namespace Project1
{
	public class IO
	{
		public Node<Process> WaitingList { get; set; }
	}

	public interface IIOManager
	{
		void Reset();
	}

	public class IOManager : IIOManager
	{
		public IOManager(IMessageBoard messageBoard)
		{
			messageBoard.Receive<RequestIOCommand>(OnRequestIOCommand);
			messageBoard.Receive<CompleteIOCommand>(OnCompleteIOCommand);
		}

		public void Reset()
		{
			
		}

		private void OnRequestIOCommand(RequestIOCommand command)
		{
			
		}

		private void OnCompleteIOCommand(CompleteIOCommand command)
		{
			
		}
	}
}
