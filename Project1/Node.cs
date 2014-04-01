using System;

namespace Project1
{
	/// <summary>
	/// Node in a doubly-linked list.
	/// </summary>
	public class Node<T>
	{
		public Node<T> Next { get; set; }
		public Node<T> Prev { get; set; }
		public T Data { get; private set; }

		public Node(T data)
		{
			Data = data;
		}

		public static void AddToBack(ref Node<T> head, Node<T> newBack)
		{
			if (head == null)
			{
				head = newBack.Next = newBack.Prev = newBack;
			}
			else
			{
				newBack.Next = head;
				newBack.Prev = head.Prev;
				head.Prev.Next = newBack;
				head.Prev = newBack;
			}
		}

		public static Node<T> Find(Node<T> head, Predicate<T> matcher)
		{
			if (head == null)
				throw new ArgumentNullException("head");
			var current = head;
			do
			{
				if (matcher(current.Data))
					return current;
			} while ((current = current.Next) != head);
			return null;
		}

		public static void Remove(ref Node<T> head, Node<T> element)
		{
			element.Prev.Next = element.Next;
			element.Next.Prev = element.Prev;
			// If element is also head, advance head to next element.
			if (head == element)
			{
				head = element.Next;
				// If element is still head, then list has only one element.
				if (head == element)
					head = null;
			}
		}

		public static void VisitAll(Node<T> head, Action<T> action)
		{
			if (head == null)
				return;
			var current = head;
			do
			{
				action(current.Data);
			}
			while ((current = current.Next) != head);
		}

		public static int Count(Node<T> head)
		{
			if (head == null)
				return 0;
			int count = 0;
			var current = head;
			do
			{
				count++;
			}
			while ((current = current.Next) != head);
			return count;
		}
	}
}