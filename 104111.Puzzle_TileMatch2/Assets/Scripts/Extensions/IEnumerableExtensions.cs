using System.Linq;
using System.Collections.Generic;

public static class IEnumerableExtensions
{
	public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> enumerable)
	{
		return enumerable.ToArray();
	}

	public static bool TryGetValue<T>(this IEnumerable<T> list, int index, out T value)
	{
		value = list.HasIndex(index) switch {
			true => list.ElementAtOrDefault(index),
			false => default(T)
		};

		return !ReferenceEquals(value, default(T));
	}

	public static bool HasIndex<T>(this IEnumerable<T> collections, int index)
	{
		return index >= 0 && index < collections?.Count();
	}
}
