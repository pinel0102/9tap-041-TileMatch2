using UnityEngine;

using System;

using Newtonsoft.Json;

[Serializable]
public record Tile(
	Guid Guid,
	int Type,
	Vector2 Position
)
{
	[JsonConstructor]
	public Tile(int type, Vector2 position) : this (Guid.NewGuid(), type, position) {}
	
	public void Deconstruct(out int type, out Vector2 position)
	{
		type = Type;
		position = Position;
	}
}
