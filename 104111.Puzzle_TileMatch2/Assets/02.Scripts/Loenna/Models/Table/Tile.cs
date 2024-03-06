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
/// <param name="BlockerICD">Blocker 내부 카운트. (Default:0 / ICD 시용시 최소 1)</param>
/// <param name="IncludeMission">[Deprecated] 골든 퍼즐 여부.</param>
[Serializable]
public record Tile(
	Guid Guid,
	int Type,
	Vector2 Position,
    BlockerType Blocker,
    int BlockerICD,
	bool IncludeMission
)
{
	[JsonConstructor]
	public Tile(Vector2 position, BlockerType blocker = BlockerType.None, int blockerICD = 0) : this (Guid.NewGuid(), 0, position, blocker, blockerICD, false) {}

	public Bounds GetBounds(float size) => new Bounds(Position, Vector2.one * size);
	
	public void Deconstruct(out int type, out Vector2 position, out BlockerType blocker, out int blockerICD)
	{
        type = Type;
		position = Position;
        blocker = Blocker;
        blockerICD = BlockerICD;
	}
}
