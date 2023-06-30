using System.Linq;
using System.Collections.Generic;

public static class ICollectionExtensions
{
	public static bool HasIndex<T>(this ICollection<T> collections, int index)
	{
		return index >= 0 && index < collections?.Count;
	}

	public static IReadOnlyList<T> AsReadOnlyList<T>(this ICollection<T> collections)
	{
		return collections.ToArray();
	}
}
