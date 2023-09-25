#nullable enable

using System;

namespace NineTap.Common
{
	[Serializable]
	public readonly struct Optional<T>
	{
		#region Operators
		public static implicit operator Optional<T>(in T value)
		{
			return new Optional<T>(hasValue: true, value);
		}

		public static explicit operator T(in Optional<T> value)
		{
			return value.Value;
		}

		public static implicit operator Optional<T>(NoneTag tag)
		{
			return new Optional<T>();
		}
		#endregion

		#region Fields
		private readonly bool m_hasValue;
		public bool HasValue => m_hasValue;

		private readonly T? m_value;
		public T Value => m_hasValue ? m_value! : throw new InvalidOperationException();
		#endregion

		#region Constructors
		internal Optional(bool hasValue, in T? value)
		{
			m_hasValue = hasValue;
			m_value = value;
		}
		#endregion

		#region Inheritances
		public override int GetHashCode()
		{
			return HashCode.Combine(m_hasValue, m_value);
		}

		public override string ToString()
		{
			return m_hasValue && m_value != null ? m_value.ToString() : "";
		}
		#endregion

		#region Methods
		public T OrElse(in T value)
		{
			return m_hasValue ? m_value! : value;
		}

		public T GetValueOrDefault(in T defaultValue)
		{
			return m_hasValue ? m_value! : defaultValue;
		}

		public Optional<T?> AsNullable()
		{
			return new Optional<T?>(m_hasValue, m_value);
		}
		#endregion
	}
}

