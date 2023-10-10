using System;

using Cysharp.Threading.Tasks;

public interface IDisposableAsyncReactiveProperty<T> : IAsyncReactiveProperty<T>, IDisposable
{
}
