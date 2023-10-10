using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

// https://github.com/Cysharp/UniTask#channel
public class AsyncMessageBroker<T> : IDisposable
{
	Channel<T> channel;

	IConnectableUniTaskAsyncEnumerable<T> multicastSource;
	IDisposable connection;

	public AsyncMessageBroker()
	{
		channel = Channel.CreateSingleConsumerUnbounded<T>();
		multicastSource = channel.Reader.ReadAllAsync().Publish();
		connection = multicastSource.Connect(); // Publish returns IConnectableUniTaskAsyncEnumerable.
	}

	public void Publish(T value)
	{
		channel.Writer.TryWrite(value);
	}

	public IUniTaskAsyncEnumerable<T> Subscribe()
	{
		return multicastSource;
	}

	public void Dispose()
	{
		channel.Writer.TryComplete();
		connection.Dispose();
	}
}
