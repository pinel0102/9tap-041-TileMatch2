using System.Linq;
using System.Collections.Generic;

public static class IEnumerableExtensions
{
	public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> enumerable)
	{
		return enumerable.ToArray();
	}
}
