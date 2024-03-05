using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 
/// </summary>
/// <param name="LayerIndex"></param>
/// <param name="Location"></param>
/// <param name="Guid"></param>
/// <param name="Icon"></param>
/// <param name="Position"></param>
/// <param name="Blocker">Blocker 종류. (Default:0)</param>
/// <param name="AddCount">추가 Tile 개수. (Default:0)</param>
/// <param name="GoldPuzzleCount">[Deprecated] 골든 퍼즐 조각 개수. (Default:-1)</param>
/// <param name="Overlaps"></param>
public record TileItemModel(
	int LayerIndex,
	LocationType Location,
	Guid Guid,
	int Icon,
	Vector2 Position,
    BlockerType Blocker,
    int AddCount,
	int GoldPuzzleCount, //없으면 -1
	List<(Guid guid, bool exist, float distance)> Overlaps
)
{
	public bool Overlapped => Overlaps.Any(x => x.exist);
	public bool InvisibleIcon => Overlaps.Any(x => x.exist && x.distance < 170f);

	public TileItemModel(
		int layerIndex,
		LocationType location,
		Guid guid,
		int icon,
		Vector2 position,
        BlockerType blocker,
        int addCount,
		int goldPuzzleCount
	) : this(
		layerIndex,
		location,
		guid,
		icon,
		position,
        blocker,
        addCount,
		goldPuzzleCount,
		new()
	)
	{

	}
}
