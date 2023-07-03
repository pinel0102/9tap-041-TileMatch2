using UnityEngine;

using System.Collections.Generic;

partial class LevelEditor
{
	public record BrushInfo
	(
		Color Color,
		float Size,
		float Snapping,
		Vector2 Position,
		IReadOnlyList<Bounds> PlacedTiles
	)
	{
		public Bounds GetBounds() => new Bounds(Position, Vector2.one * Size);

		public bool Overlap(Bounds other)
		{
			Bounds bounds = GetBounds();
			return bounds.min.x >= other.min.x &&
				bounds.min.y >= other.min.y &&
				bounds.max.x <= other.max.x &&
				bounds.max.y <= other.max.y;
		}
	}

	public record BrushWidgetInfo
	(
		bool Interactable,
		bool Drawable,
		Vector2 LocalPosition
	);
}
