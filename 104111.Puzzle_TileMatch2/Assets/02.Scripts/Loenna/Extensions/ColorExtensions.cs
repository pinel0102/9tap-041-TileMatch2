using UnityEngine;

public static class ColorExtensions
{
	public static void Deconstruct(this in Color color, out float r, out float g, out float b, out float a)
	{
		r = color.r;
		g = color.g;
		b = color.b;
		a = color.a;
	}

	public static Color WithR(this in Color color, float value)
	{
		Color c = color;
		c.r = value;
		return c;
	}

	public static Color WithG(this in Color color, float value)
	{
		Color c = color;
		c.g = value;
		return c;
	}

	public static Color WithB(this in Color color, float value)
	{
		Color c = color;
		c.b = value;
		return c;
	}

	public static Color WithA(this in Color color, float value)
	{
		Color c = color;
		c.a = value;
		return c;
	}
}

