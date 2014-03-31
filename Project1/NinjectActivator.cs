using System;
using Ninject;

namespace Project1
{
	public interface IActivator
	{
		object Create(Type type);
	}

	public class NinjectActivator : IActivator
	{
		private readonly IKernel kernel;

		public NinjectActivator(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public object Create(Type type)
		{
			return kernel.Get(type);
		}
	}
}