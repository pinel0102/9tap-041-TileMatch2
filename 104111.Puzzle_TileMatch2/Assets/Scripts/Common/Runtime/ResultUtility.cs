#nullable enable

namespace NineTap.Common
{
	public static class ResultUtility
	{
		public static OkResult<Unit> Ok()
		{
			return new OkResult<Unit>(new Unit());
		}

		public static OkResult<T> Ok<T>(T value)
		{
			return new OkResult<T>(value);
		}

		public static ErrorResult<Unit> Error()
		{
			return new ErrorResult<Unit>(new Unit());
		}

		public static ErrorResult<TError> Error<TError>(TError value)
		{
			return new ErrorResult<TError>(value);
		}
	}
}
