using System;
using System.Collections.Generic;

namespace Project1
{
	public interface IMessageBoard
	{
		void Send<T>(T message);
		void Receive<T>(Action<T> handler);
	}

	public class MessageBoard : IMessageBoard
	{
		private readonly Dictionary<Type, List<object>> typeHandlers = new Dictionary<Type, List<object>>(); 

		public void Send<T>(T message)
		{
			List<object> handlers;
			if(typeHandlers.TryGetValue(typeof(T), out handlers))
				foreach (var handler in handlers)
					((Action<T>) handler)(message);
		}

		public void Receive<T>(Action<T> handler)
		{
			List<object> handlers;
			if (!typeHandlers.TryGetValue(typeof (T), out handlers))
				typeHandlers.Add(typeof (T), handlers = new List<object>());
			handlers.Add(handler);
		}
	}
}