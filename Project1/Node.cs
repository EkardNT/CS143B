namespace Project1
{
	/// <summary>
	/// Node in a singly-linked list.
	/// </summary>
	public class Node<T>
	{
		public Node<T> Next { get; set; }
		public T Data { get; private set; }

		public Node(T data)
		{
			Data = data;
		}
	}
}