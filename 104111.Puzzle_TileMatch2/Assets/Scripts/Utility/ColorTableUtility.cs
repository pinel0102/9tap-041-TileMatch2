using UnityEngine;

using System.Collections.Generic;

public static class ColorTableUtility
{
	private static readonly List<Color> m_colorTables = new();

	public static void Create()
	{
		// 컬러 테이블 만들기
		var hues =  GetFloatList(0.1f, 1f, 5);
		var saturations = GetFloatList(0.1f, 0.9f, 5);
		var values = GetFloatList(0.5f, 1f, 5);

		values.ForEach(
			value => saturations.ForEach(
				saturation => hues.ForEach(
					hue => m_colorTables.Add(Color.HSVToRGB(H: hue, S: saturation, V: value))
				)
			)
		);

		static List<float> GetFloatList(float min, float max, float split)
		{
			List<float> value = new();
			for (float i = 0; i < split; i++)
			{
				float lerp = Mathf.Lerp(min, max, (float)(i + 1) / split);
				value.Add(lerp);
			}
			return value;
		}
	}

	public static Color GetColor(int index)
	{
		if (m_colorTables.Count <= 0)
		{
			Create();
		}

		if (m_colorTables.HasIndex(index))
		{
			return m_colorTables[index];
		}

		return Color.gray;
	}
}
