using System;
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

	/// <summary>
	/// 리스트의 마지막 요소를 <paramref name="index"/> 위치로 이동하여 기존 값을 대체한다.
	/// </summary>
	public static void RemoveAtSwap<T>(this IList<T> list, int index)
	{
		if (list == null)
		{
			return;
		}

		if (index < 0 || index >= list.Count)
		{
			return;
		}

		int lastIndex = list.Count - 1;
		list[index] = list[lastIndex];

		list.RemoveAt(lastIndex);
	}

	/// <summary>
	/// Knuth 셔플
	/// </summary>
	public static IList<T> Shuffle<T>(this IList<T> list)
	{
		if (list == null)
		{
			return list;
		}

		int count = list.Count;

		if (count < 2)
		{
			return list;
		}

		for (int i = 0; i < count - 1; i++) // 0 ~ (count-2)
		{
			int randomIndex = UnityEngine.Random.Range(i, count);
			SwapUnsafe(list, i, randomIndex);
		}

		return list;
	}

	private static void SwapUnsafe<T>(IList<T> list, int index1, int index2)
	{
		(list[index1], list[index2]) = (list[index2], list[index1]);
	}
}
