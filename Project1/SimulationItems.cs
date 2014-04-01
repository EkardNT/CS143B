using System.Collections.Generic;

namespace Project1
{
	public class AccessRequest
	{
		public readonly Process Process;
		public readonly int Amount;

		public AccessRequest(Process process, int amount)
		{
			Process = process;
			Amount = amount;
		}
	}
	
	public class Resource
	{
		public Resource(string name, int total)
		{
			Name = name;
			Total = Available = total;
			Available = total;
		}

		// Resource's name.
		public readonly string Name;
		// Total number of resources of this type existing in the system.
		public readonly int Total;
		// Quantity of resources of this type currently available.
		public int Available;
		// Processes which are waiting for access to this resource.
		public Node<AccessRequest> WaitingList;
	}

	public enum ProcessStatus
	{
		Running,
		Ready,
		Waiting
	}

	public class Process
	{
		public Process(string name, Process parent, int priority)
		{
			Name = name;
			Parent = parent;
			Priority = priority;
			HeldResources = new Dictionary<string, int>();
		}

		// Process' name.
		public readonly string Name;
		// The Process that was created this Process.
		public readonly Process Parent;
		// Process' priority, in [0, 2].
		public readonly int Priority;
		// Head node in the linked list of Processes this
		// Process has created.
		public Node<Process> ChildList;
		// The process' status. This value indicates which
		// queue the WaitingNode node belongs to.
		public ProcessStatus Status;
		// The node referencing this Process in the ready
		// queue. Is null if this Process' status is Waiting.
		public Node<Process> ReadyNode; 
		// The node referencing this Process in a resource
		// waiting queue. Is null if this Process' status
		// is not Waiting.
		public Node<AccessRequest> WaitingNode;
		// Name of the resource this Process is waiting for
		// access to. This is used to remove the Process 
		// from the correct resource waiting queue if it
		// is destroyed while waiting.
		public string WaitingResourceName;
		// Number of resources of each type held by this 
		// Process.
		public readonly Dictionary<string, int> HeldResources;
	}
}