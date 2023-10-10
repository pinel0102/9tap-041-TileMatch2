#nullable enable

namespace NineTap.Common
{
	public static class Optional
	{
		public static Optional<T> Of<T>(T value)
		{
			return new Optional<T>(hasValue: true, value);
		}
	}
}
