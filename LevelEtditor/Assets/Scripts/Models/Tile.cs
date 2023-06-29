using UnityEngine;

using System;

[Serializable]
public record Tile(
	int Type,
	Vector2 Position
)
{
	public void Deconstruct(out int type, out Vector2 position)
	{
		type = Type;
		position = Position;
	}
}
