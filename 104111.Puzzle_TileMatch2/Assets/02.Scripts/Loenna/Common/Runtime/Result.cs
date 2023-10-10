namespace NineTap.Common
{
	public static class Result
	{
		public static Result<T, TError> OfValue<T, TError>(T value)
		{
			return new Result<T, TError>(tag: true, value, error: default);
		}

		public static Result<T, TError> OfError<T, TError>(TError error)
		{
			return new Result<T, TError>(tag: false, value: default, error);
		}
	}
}
