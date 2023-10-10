#nullable enable

using System;

namespace NineTap.Common
{
	[Serializable]
	public readonly struct Result<T, TError>
	{
		#region Types
		private enum TagInternal
		{
			None,

			Ok,
			Error,
		}
		#endregion

		#region Fields
		private readonly TagInternal m_tag;
		public bool Tag => m_tag switch
		{
			TagInternal.Ok => true,
			TagInternal.Error => false,
			_ => throw new InvalidOperationException()
		};

		private readonly T? m_value;
		public T Value => Tag ? m_value! : throw new InvalidOperationException();

		private readonly TError? m_error;
		public TError Error => !Tag ? m_error! : throw new InvalidOperationException();
		#endregion

		#region Constructors
		internal Result(bool tag, T? value, TError? error)
		{
			m_tag = tag ? TagInternal.Ok : TagInternal.Error;
			m_value = value;
			m_error = error;
		}
		#endregion

		#region Cast Operators
		public static implicit operator Result<T, TError>(OkResult<T> result)
		{
			return new Result<T, TError>(true, result.Value, default);
		}

		public static implicit operator Result<T, TError>(ErrorResult<TError> result)
		{
			return new Result<T, TError>(false, default, result.Error);
		}
		#endregion
	}
}
