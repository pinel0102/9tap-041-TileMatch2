using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public abstract partial class Command
{
	public class Invoker
	{
		private readonly Stack<ICommand> m_commandHistories;
		private IAsyncReactiveProperty<bool> m_notEmpty;
		public IReadOnlyAsyncReactiveProperty<bool> NotEmpty => m_notEmpty;

		public Invoker()
		{
            m_commandHistories = new();
			m_notEmpty = new AsyncReactiveProperty<bool>(false).WithDispatcher();
		}

		public void SetCommand(ICommand command)
		{
			m_commandHistories.Push(command);
			m_notEmpty.Update(flag => flag = true);
		}

		public void Execute()
		{
			if (m_commandHistories.TryPeek(out ICommand command))
			{
				command.Execute();
			}
		}

		public void UnExecute()
		{
            if (m_commandHistories.TryPop(out ICommand command))
			{
                command.UnExecute();
				m_notEmpty.Update(flag => m_commandHistories.Count > 0);
			}
		}

		public void ClearHistories()
		{
            m_commandHistories.Clear();
			m_notEmpty.Update(flag => flag = false);
		}
	}
}
