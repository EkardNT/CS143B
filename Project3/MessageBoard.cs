using System;
using System.Collections.Generic;
namespace Project3
{
	public class MessageBoard
	{
		private readonly Dictionary<Type, List<Action<object>>> typeHandlers = new Dictionary<Type, List<Action<object>>>();

		public void Send(object message)
		{
			List<Action<object>> handlers;
			var type = message.GetType();
			if (typeHandlers.TryGetValue(type, out handlers))
				foreach (var handler in handlers)
					handler(message);
		}

		public void Receive<T>(Action<T> handler)
		{
			List<Action<object>> handlers;
			if (!typeHandlers.TryGetValue(typeof(T), out handlers))
				typeHandlers.Add(typeof(T), handlers = new List<Action<object>>());
			handlers.Add(message => handler((T)message));
		}
	}
}