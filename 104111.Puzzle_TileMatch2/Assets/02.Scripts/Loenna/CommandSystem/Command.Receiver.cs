using Cysharp.Threading.Tasks;

public abstract partial class Command
{
	public abstract class Receiver<T>
	{
		public abstract UniTask Execute(T item);
		public abstract UniTask UnExecute(T Item);
	}
}
