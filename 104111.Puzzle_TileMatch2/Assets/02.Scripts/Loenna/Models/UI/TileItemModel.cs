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
/// <param name="Icon">현재 Icon (Default:IconList.Last())</param>
/// <param name="IconList">타일에 적용되는 Icon List (Suitcase : Max ICD) (Default:{ Icon })</param>
/// <param name="Position"></param>
/// <param name="BlockerType">Blocker 종류. (Default:0)</param>
/// <param name="BlockerICD">Blocker 내부 카운트. (Default:0 / ICD 시용시 최소 1)</param>
/// <param name="GoldPuzzleCount">[Deprecated] 골든 퍼즐 조각 개수. (Default:-1)</param>
/// <param name="Overlaps"></param>
public record TileItemModel(
	int LayerIndex,
    int SiblingIndex,
	LocationType Location,
	Guid Guid,
	int Icon,
    List<int> IconList,
	Vector2 Position,
    BlockerType BlockerType,
    int BlockerICD,
	int GoldPuzzleCount, //없으면 -1
	List<(Guid guid, bool exist, float distance)> Overlaps
)
{
	public bool Overlapped => Overlaps.Any(x => x.exist);
	public bool InvisibleIcon => Overlaps.Any(x => x.exist && x.distance < 170f);

	public TileItemModel(
		int layerIndex,
        int siblingIndex,
		LocationType location,
		Guid guid,
		int icon,
        List<int> iconList,
		Vector2 position,
        BlockerType blockerType,
        int blockerICD,
		int goldPuzzleCount
	) : this(
		layerIndex,
        siblingIndex,
		location,
		guid,
		icon,
        iconList,
		position,
        blockerType,
        blockerICD,
		goldPuzzleCount,
		new()
	)
	{

	}
}
