using UnityEngine;

using Axis = UnityEngine.RectTransform.Axis;

public static class RectTransformExtensions
{
	public static void SetSize(this RectTransform rectTransform, float size)
	{
		rectTransform.SetSizeWithCurrentAnchors(Axis.Horizontal, size);
		rectTransform.SetSizeWithCurrentAnchors(Axis.Vertical, size);
	}

	public static void SetSize(this RectTransform rectTransform, Vector2 size)
	{
		rectTransform.SetSizeWithCurrentAnchors(Axis.Horizontal, size.x);
		rectTransform.SetSizeWithCurrentAnchors(Axis.Vertical, size.y);
	}
}
