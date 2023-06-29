using System.Collections.Generic;

public static class ArrayExtensions
{
	public static IReadOnlyList<T> AsReadOnlyList<T>(this T[] array)
	{
		return array;
	}
}
