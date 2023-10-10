using UnityEngine;

using Axis = UnityEngine.RectTransform.Axis;

public static class RectTransformExtensions
{
	public static Bounds CreateLocalBounds(this RectTransform rectTransform)
	{
		return new Bounds(rectTransform.localPosition, rectTransform.rect.size);
	}

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

	public static void SetAnchorMin(this RectTransform rectTransform, Vector2 anchorMin)
	{
		rectTransform.anchorMin = anchorMin;
	}

	public static void SetAnchorMax(this RectTransform rectTransform, Vector2 anchorMax)
	{
		rectTransform.anchorMax = anchorMax;
	}

	public static void SetOffsetMinX(this RectTransform rectTransform, float x)
	{
		Vector2 offsetMin = rectTransform.offsetMin;
		offsetMin.x = x;
		rectTransform.offsetMin = offsetMin;
	}

	public static void AddOffsetMinX(this RectTransform rectTransform, float x)
	{
		Vector2 offsetMin = rectTransform.offsetMin;
		offsetMin.x += x;
		rectTransform.offsetMin = offsetMin;
	}

	public static void SetOffsetMinY(this RectTransform rectTransform, float y)
	{
		Vector2 offsetMin = rectTransform.offsetMin;
		offsetMin.y = y;
		rectTransform.offsetMin = offsetMin;
	}

	public static void AddOffsetMinY(this RectTransform rectTransform, float y)
	{
		Vector2 offsetMin = rectTransform.offsetMin;
		offsetMin.y += y;
		rectTransform.offsetMin = offsetMin;
	}

	public static void SetOffsetMaxX(this RectTransform rectTransform, float x)
	{
		Vector2 offsetMax = rectTransform.offsetMax;
		offsetMax.x = x;
		rectTransform.offsetMax = offsetMax;
	}

	public static void AddOffsetMaxX(this RectTransform rectTransform, float x)
	{
		Vector2 offsetMax = rectTransform.offsetMax;
		offsetMax.x += x;
		rectTransform.offsetMax = offsetMax;
	}

	public static void SetOffsetMaxY(this RectTransform rectTransform, float y)
	{
		Vector2 offsetMax = rectTransform.offsetMax;
		offsetMax.y = y;
		rectTransform.offsetMax = offsetMax;
	}

	public static void AddOffsetMaxY(this RectTransform rectTransform, float y)
	{
		Vector2 offsetMax = rectTransform.offsetMax;
		offsetMax.y += y;
		rectTransform.offsetMax = offsetMax;
	}

	/// <summary>
	/// RectTransform [Left: min, Right: max]
	/// </summary>
	/// <param name="rectTransform"></param>
	/// <param name="value"></param>
	public static void SetOffsetX(this RectTransform rectTransform, float min, float max)
	{
		SetOffsetMinX(rectTransform, min);
		SetOffsetMaxX(rectTransform, max);
	}

	/// <summary>
	/// RectTransform [Bottom: min, Top: max]
	/// </summary>
	/// <param name="rectTransform"></param>
	/// <param name="value"></param>
	public static void SetOffsetY(this RectTransform rectTransform, float min, float max)
	{
		SetOffsetMinY(rectTransform, min);
		SetOffsetMaxY(rectTransform, max);
	}

	public static void SetStretch(this RectTransform rectTransform, float left = 0f, float right = 0f, float bottom = 0f, float top = 0f)
	{
		SetAnchorMin(rectTransform, Vector2.zero);
		SetAnchorMax(rectTransform, Vector2.one);
		SetOffsetX(rectTransform, left, right);
		SetOffsetY(rectTransform, bottom, top);
	}
}
