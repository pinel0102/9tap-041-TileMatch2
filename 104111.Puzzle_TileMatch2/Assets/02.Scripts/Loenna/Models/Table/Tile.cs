using UnityEngine;

using System;

using Newtonsoft.Json;

[Serializable]
public record Tile(
	Guid Guid,
	int Type,
	Vector2 Position,
	bool IncludeMission
)
{
	[JsonConstructor]
	public Tile(int type, Vector2 position) : this (Guid.NewGuid(), type, position, false) {}

	public Bounds GetBounds(float size) => new Bounds(Position, Vector2.one * size);
	
	public void Deconstruct(out int type, out Vector2 position)
	{
		type = Type;
		position = Position;
	}
}
