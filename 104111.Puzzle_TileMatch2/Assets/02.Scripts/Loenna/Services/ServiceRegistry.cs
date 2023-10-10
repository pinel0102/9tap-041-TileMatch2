#nullable enable

using UnityEngine;
using UnityEngine.Assertions;

using System;
using System.Collections.Generic;

public class ServiceRegistry<T>
{
	#region Types
	private class Service
	{
		#region Fields
		private readonly Type m_type;
		public Type Type => m_type;

		private readonly string m_tag;
		public string Tag => m_tag;

		private readonly T m_implementation;
		public T Implementation => m_implementation;
		#endregion

		#region Constructors
		public Service(Type type, string tag, T implementation)
		{
			m_type = type;
			m_tag = tag;
			m_implementation = implementation;
		}
		#endregion
	}
	#endregion

	#region Static
	private static int IndexOf(IList<Service> serviceList, Type type, string tag)
	{
		for (int i = 0; i < serviceList.Count; ++i)
		{
			Service service = serviceList[i];
			if (type.IsAssignableFrom(service.Type) && service.Tag.Equals(tag))
			{
				return i;
			}
		}

		return -1;
	}

	private static bool IsNullOrDestroyed(object? value)
	{
		return value is UnityEngine.Object obj ? !obj : value == null;
	}
	#endregion

	#region Fields
	private readonly List<Service> m_serviceList = new();
	#endregion

	#region Methods
	/// <summary>
	/// <typeparamref name="TService"/> 타입에 대한 서비스를 반환한다.
	/// </summary>
	public TService? Get<TService>(string tag = "")
		where TService : T
	{
		int index = IndexOf(m_serviceList, typeof(TService), tag);
		return index != -1
			? (TService?)m_serviceList[index].Implementation
			: default;
	}

	/// <summary>
	/// <typeparamref name="TService"/> 타입에 대한 서비스를 등록한다.
	/// </summary>
	public void Register<TService>(TService service, string tag = "")
		where TService : T
	{
		Register(typeof(TService), service, tag);
	}

	/// <summary>
	/// <typeparamref name="TService"/> 타입에 대한 서비스를 제거한다. 해당 타입의 서비스가 없는 경우 아무것도 하지 않는다.
	/// </summary>
	public void Unregister<TService>(string tag = "")
		where TService : T
	{
		Unregister(typeof(TService), tag);
	}

	private void Register(Type type, T implementation, string tag)
	{
		Assert.IsFalse(IsNullOrDestroyed(implementation));
		Assert.IsTrue(type.IsInstanceOfType(implementation));

		for (int i = 0; i < m_serviceList.Count; ++i)
		{
			// 서비스 오브젝트는 중복될 수 있지만 타입과 태그 쌍은 중복될 수 없고 Implementation 값이 없다면 등록되지 않은 것으로 판단한다.
			Service service = m_serviceList[i];
			if (service.Type == type && !IsNullOrDestroyed(service.Implementation))
			{
				Debug.LogError($"{type.Name} 타입에 대한 서비스를 중복 등록할 수 없습니다.");
				return;
			}
		}

		m_serviceList.Add(new Service(type, tag, implementation));
	}

	private void Unregister(Type type, string tag)
	{
		int index = IndexOf(m_serviceList, type, tag);
		if (index < 0 || index >= m_serviceList.Count)
		{
			return;
		}

		m_serviceList.RemoveAtSwap(index);
	}

	public void Dispose()
	{
		m_serviceList.ForEach(
			service => {
				if (service.Implementation is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		);
	}
	#endregion
}
