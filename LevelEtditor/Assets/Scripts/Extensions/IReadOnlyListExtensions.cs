using System.Collections.Generic;

public static class IReadOnlyListExtensions
{
    public static bool HasIndexReadOnly<T>(this IReadOnlyList<T> list, int index)
	{
		return index >= 0 && index < list?.Count;
	}
}
