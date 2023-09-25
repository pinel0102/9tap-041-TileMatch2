#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Cysharp.Threading.Tasks;

public static class IAsyncReactivePropertyExtensions
{
	private abstract class AsyncReactivePropertyWrapperBase<T> : IDisposableAsyncReactiveProperty<T>
	{
		#region Fields
		private readonly IAsyncReactiveProperty<T> m_property;
		private readonly IDisposable? m_disposable;
		#endregion

		#region Constructors
		protected AsyncReactivePropertyWrapperBase(IAsyncReactiveProperty<T> property)
		{
			m_property = property;
			m_disposable = property as IDisposable;
		}
		#endregion

		#region IAsyncReactiveProperty<T> Interface
		public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			return m_property.GetAsyncEnumerator(cancellationToken);
		}

		public virtual T Value
		{
			get => m_property.Value;
			set => m_property.Value = value;
		}

		public IUniTaskAsyncEnumerable<T> WithoutCurrent()
		{
			return m_property.WithoutCurrent();
		}

		public UniTask<T> WaitAsync(CancellationToken cancellationToken = default)
		{
			return m_property.WaitAsync(cancellationToken);
		}
		#endregion

		#region IDisposable Interface
		public void Dispose()
		{
			m_disposable?.Dispose();
		}
		#endregion
	}

	private class AsyncReactiveProperty_EqualityComparer<T> : AsyncReactivePropertyWrapperBase<T>
	{
		#region Types
		private class FunctionEqualityComparer : IEqualityComparer<T>
		{
			#region Fields
			private readonly Func<T, T, bool> m_function;
			#endregion

			#region Constructors
			public FunctionEqualityComparer(Func<T, T, bool> function)
			{
				m_function = function;
			}
			#endregion

			#region IEqualityComparer<T> Interface
			public bool Equals(T x, T y)
			{
				return m_function.Invoke(x, y);
			}

			public int GetHashCode(T obj)
			{
				return obj?.GetHashCode() ?? 0;
			}
			#endregion
		}
		#endregion

		#region Fields
		private readonly IEqualityComparer<T> m_equalityComparer;
		#endregion

		#region Constructors
		public AsyncReactiveProperty_EqualityComparer(
			IAsyncReactiveProperty<T> property,
			Func<T, T, bool> equalityComparer
		) : this(property, new FunctionEqualityComparer(equalityComparer))
		{
		}

		public AsyncReactiveProperty_EqualityComparer(
			IAsyncReactiveProperty<T> property,
			IEqualityComparer<T> equalityComparer
		) : base(property)
		{
			m_equalityComparer = equalityComparer;
		}
		#endregion

		#region Inheritances
		public override T Value
		{
			set
			{
				if (m_equalityComparer.Equals(base.Value, value))
				{
					return;
				}

				base.Value = value;
			}
		}
		#endregion
	}

	private class AsyncReactiveProperty_BeforeSetValue<T> : AsyncReactivePropertyWrapperBase<T>
	{
		#region Fields
		private readonly Func<T, T, T> m_beforeSetValue;
		#endregion

		#region Constructors
		public AsyncReactiveProperty_BeforeSetValue(
			IAsyncReactiveProperty<T> property,
			Func<T, T, T> beforeSetValue
		) : base(property)
		{
			m_beforeSetValue = beforeSetValue;
		}
		#endregion

		#region IAsyncReactiveProperty<T> Interface
		public override T Value
		{
			set => base.Value = m_beforeSetValue.Invoke(base.Value, value);
		}
		#endregion
	}

	private class AsyncReactiveProperty_AfterSetValue<T> : AsyncReactivePropertyWrapperBase<T>
	{
		#region Fields
		private readonly Action<T> m_afterSetValue;
		#endregion

		#region Constructors
		public AsyncReactiveProperty_AfterSetValue(
			IAsyncReactiveProperty<T> property,
			Action<T> afterSetValue
		) : base(property)
		{
			m_afterSetValue = afterSetValue;
		}
		#endregion

		#region IAsyncReactiveProperty<T> Interface
		public override T Value
		{
			set
			{
				base.Value = value;
				m_afterSetValue.Invoke(value);
			}
		}
		#endregion
	}

	private class AsyncReactiveProperty_Dispatcher<T> : AsyncReactivePropertyWrapperBase<T>
	{
		#region Fields
		private bool m_isDispatching;
		private readonly Queue<T> m_queue = new();
		#endregion

		#region Constructors
		public AsyncReactiveProperty_Dispatcher(IAsyncReactiveProperty<T> property) : base(property)
		{
		}
		#endregion

		#region IAsyncReactiveProperty<T> Interface
		public override T Value
		{
			set
			{
				m_queue.Enqueue(value);

				if (m_isDispatching)
				{
					return;
				}

				while (m_queue.Count > 0)
				{
					try
					{
						m_isDispatching = true;
						base.Value = m_queue.Dequeue();
					}
					finally
					{
						m_isDispatching = false;
					}
				}
			}
		}
		#endregion
	}

	public static T Update<T>(this IAsyncReactiveProperty<T> property, Func<T, T> selector)
	{
		T value = selector.Invoke(property.Value);
		property.Value = value;

		return value;
	}

	public static IDisposableAsyncReactiveProperty<T> WithFilter<T>(
		this IAsyncReactiveProperty<T> property,
		Func<T, bool> filter
	)
	{
		return new AsyncReactiveProperty_EqualityComparer<T>(
			property,
			equalityComparer: (oldValue, newValue) => filter.Invoke(newValue)
		);
	}

	public static IDisposableAsyncReactiveProperty<T> WithEqualityComparer<T>(
		this IAsyncReactiveProperty<T> property,
		Func<T, T, bool> equalityComparer
	)
	{
		return new AsyncReactiveProperty_EqualityComparer<T>(property, equalityComparer);
	}

	public static IDisposableAsyncReactiveProperty<T> WithEqualityComparer<T>(
		this IAsyncReactiveProperty<T> property,
		IEqualityComparer<T> equalityComparer
	)
	{
		return new AsyncReactiveProperty_EqualityComparer<T>(property, equalityComparer);
	}

	public static IDisposableAsyncReactiveProperty<T> WithBeforeSetValue<T>(
		this IAsyncReactiveProperty<T> property,
		Func<T, T, T> beforeSetValue
	)
	{
		return new AsyncReactiveProperty_BeforeSetValue<T>(property, beforeSetValue);
	}

	public static IDisposableAsyncReactiveProperty<T> WithAfterSetValue<T>(
		this IAsyncReactiveProperty<T> property,
		Action<T> afterSetValue
	)
	{
		return new AsyncReactiveProperty_AfterSetValue<T>(property, afterSetValue);
	}

	public static IDisposableAsyncReactiveProperty<T> WithDispatcher<T>(this IAsyncReactiveProperty<T> property)
	{
		return new AsyncReactiveProperty_Dispatcher<T>(property);
	}
}
