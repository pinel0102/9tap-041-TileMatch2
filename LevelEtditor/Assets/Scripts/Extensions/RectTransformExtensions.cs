#define ENABLE_DEBUG_LOG
using UnityEngine;

using Axis = UnityEngine.RectTransform.Axis;

public static class RectTransformExtensions
{
	public static Bounds GetBounds(this RectTransform content, RectTransform viewport, float scale = 1f)
	{
		if (content == null || viewport == null)
		{
			return new Bounds();
		}

		Vector2 vMin = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 vMax = new Vector2(float.MinValue, float.MinValue);

		Matrix4x4 viewportLocalMatrix = viewport.worldToLocalMatrix;
		Vector3[] corners = new Vector3[4];
		content.GetWorldCorners(corners);

		foreach (Vector3 corner in corners)
		{
			Vector3 vector = viewportLocalMatrix.MultiplyPoint3x4(corner);
			vMin = Vector2.Min(vector, vMin);
			vMax = Vector2.Max(vector, vMax);
		}

		Bounds bounds = new Bounds(vMin, Vector3.zero);
		bounds.Encapsulate(vMax);
		bounds.size *= scale;

		#if ENABLE_DEBUG_LOG
		var p1 = new Vector2(vMin.x, vMin.y);
		var p2 = new Vector2(vMax.x, vMin.y);
		var p3 = new Vector2(vMax.x, vMax.y);
		var p4 = new Vector2(vMin.x, vMax.y);

		Debug.DrawLine(p1, p2, Color.yellow);
		Debug.DrawLine(p2, p3, Color.yellow);
		Debug.DrawLine(p3, p4, Color.yellow);
		Debug.DrawLine(p4, p1, Color.yellow);
		#endif

		return bounds;
	}

	public static void SetSize(this RectTransform rectTransform, float size)
	{
		rectTransform.SetSizeWithCurrentAnchors(Axis.Horizontal, size);
		rectTransform.SetSizeWithCurrentAnchors(Axis.Vertical, size);
	}
}
