namespace Project1
{
	public interface ICommand
	{
		string Name { get; }
		string Description { get; }
		void Execute(string[] parameters);
	}
}