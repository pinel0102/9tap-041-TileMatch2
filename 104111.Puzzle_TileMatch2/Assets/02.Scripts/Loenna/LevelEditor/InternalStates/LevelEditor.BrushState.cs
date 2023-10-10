using UnityEngine;

using System;

partial class LevelEditor
{
	public abstract record BrushInfo(BrushMode Mode, float Size, Vector2 Position)
	{
		public Bounds GetBounds() => new Bounds(Position, Vector2.one * Size);

		public virtual bool Overlap(Bounds other)
		{
			Bounds bounds = GetBounds();
			return bounds.min.x >= other.min.x &&
				bounds.min.y >= other.min.y &&
				bounds.max.x <= other.max.x &&
				bounds.max.y <= other.max.y;
		}
	}

	public record TileBrushInfo
	(
		float Size,
		float Snapping,
		Vector2 Position,
		DrawOrder DrawOrder
	) : BrushInfo(BrushMode.TILE_BRUSH, Size, Position)
	{
		
	}

	public record MissionStampInfo
	(
		float Size,
		Vector2 NearBy,
		Vector2 Position,
		int LayerIndex,
		Guid TileGuid
	) : BrushInfo(BrushMode.MISSION_STAMP, Size, Position)
	{
		public MissionStampInfo(float size, Vector2 position, int layerIndex) : this(size, Vector2.zero, position, layerIndex, Guid.Empty) {}

		public override bool Overlap(Bounds other)
		{
			if (base.Overlap(other))
			{
				return Vector2.Distance(Position, NearBy) <= Size * 0.5f;
			}

			return false;
		}
	}

	public record BrushWidgetInfo
	(
		BrushMode Mode,
		bool Interactable,
		Vector2 LocalPosition
	);
}
