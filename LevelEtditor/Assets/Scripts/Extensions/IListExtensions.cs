using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IListExtensions
{
	public static bool TryGetValue<T>(this IList<T> list, int index, out T value)
	{
		value = list.HasIndex(index) switch {
			true => list[index],
			false => default(T)
		};

		return !ReferenceEquals(value, default(T));
	}
}
