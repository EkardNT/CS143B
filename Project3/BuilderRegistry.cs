using System;
using System.Collections.Generic;
using System.Linq;

namespace Project3
{
	public class BuilderRegistry
	{
		private readonly Dictionary<string, CommandBuilder> builders;

		public BuilderRegistry()
		{
			builders = typeof(BuilderRegistry).Assembly.DefinedTypes
				.Where(type => type != typeof(CommandBuilder))
				.Where(type => typeof(CommandBuilder).IsAssignableFrom(type))
				.Select(Activator.CreateInstance)
				.Cast<CommandBuilder>()
				.ToDictionary(builder => builder.ShellCommand);
		}

		public bool TryGetBuilder(string shellCommand, out CommandBuilder builder)
		{
			return builders.TryGetValue(shellCommand, out builder);
		}
	}
}