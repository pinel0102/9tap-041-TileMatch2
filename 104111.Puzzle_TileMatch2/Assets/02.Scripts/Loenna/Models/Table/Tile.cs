using UnityEngine;

using System;

using Newtonsoft.Json;

/// <summary>
/// 
/// </summary>
/// <param name="Guid"></param>
/// <param name="Type">[No Use]</param>
/// <param name="Position"></param>
/// <param name="Blocker">Blocker 종류. (Default:0)</param>
/// <param name="TileCount">Tile 개수. (Default:1)</param>
/// <param name="IncludeMission">[Deprecated] 골든 퍼즐 여부.</param>
[Serializable]
public record Tile(
	Guid Guid,
	int Type,
	Vector2 Position,
    int Blocker,
    int TileCount,    
	bool IncludeMission
)
{
	[JsonConstructor]
	public Tile(Vector2 position, int blocker = 0, int tileCount = 1) : this (Guid.NewGuid(), 0, position, blocker, tileCount, false) {}

	public Bounds GetBounds(float size) => new Bounds(Position, Vector2.one * size);
	
	public void Deconstruct(out int type, out Vector2 position, out int blocker, out int tileCount)
	{
        type = Type;
		position = Position;
        blocker = Blocker;
        tileCount = TileCount;
	}
}
