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
/// <param name="AddCount">추가 Tile 개수. (Default:0)</param>
/// <param name="IncludeMission">[Deprecated] 골든 퍼즐 여부.</param>
[Serializable]
public record Tile(
	Guid Guid,
	int Type,
	Vector2 Position,
    BlockerType Blocker,
    int AddCount,    
	bool IncludeMission
)
{
	[JsonConstructor]
	public Tile(Vector2 position, BlockerType blocker = BlockerType.None, int addCount = 0) : this (Guid.NewGuid(), 0, position, blocker, addCount, false) {}

	public Bounds GetBounds(float size) => new Bounds(Position, Vector2.one * size);
	
	public void Deconstruct(out int type, out Vector2 position, out BlockerType blocker, out int addCount)
	{
        type = Type;
		position = Position;
        blocker = Blocker;
        addCount = AddCount;
	}
}
