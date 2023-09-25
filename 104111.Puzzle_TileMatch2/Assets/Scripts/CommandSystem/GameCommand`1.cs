public class GameCommand<T> : ICommand
{
	private readonly T m_resource;
	private readonly Command.Receiver<T>  m_receiver;

	public GameCommand(Command.Receiver<T> receiver, T resource)
	{
		m_receiver = receiver;
		m_resource = resource;
	}

	public void Execute()
	{
		m_receiver?.Execute(m_resource);
	}

	public void UnExecute()
	{
		m_receiver?.UnExecute(m_resource);
	}
}

public static class DoNothing<T>
{
	public static ICommand Command = new GameCommand<T>(null, default);
}


