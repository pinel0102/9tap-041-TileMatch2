using System.Collections.Generic;
using System.Diagnostics;

public class ConcurrentCommand : ICommand
{
	private readonly List<ICommand> m_commands;

	public ConcurrentCommand(params ICommand[] commands)
	{
		m_commands = new();

		if (commands?.Length > 0)
		{
			foreach (var command in commands)
			{
				AddCommand(command);
			}
		}
	}

	public void AddCommand(params ICommand[] commands)
	{
		m_commands.AddRange(commands);
	}

	public void InsertCommand(int index, ICommand command)
	{
		if (index < 0 || index > m_commands.Count)
		{
			return;
		}
		m_commands.Insert(index, command);
	}

	public void Execute()
	{
		foreach (ICommand command in m_commands)
		{
            command.Execute();
		}
	}

	public void UnExecute()
	{
		var stack = new Stack<ICommand>(m_commands);
		while (stack.Count > 0)
		{
			ICommand command = stack.Pop();
			command.UnExecute();
		}
	}
}
