#nullable enable
using NineTap.Common;

public static class OptionalUtility
{
	public static Optional<T> Some<T>(in T value)
	{
		return Optional.Of(value);
	}

	public static NoneTag None()
	{
		return new NoneTag();
	}
}
