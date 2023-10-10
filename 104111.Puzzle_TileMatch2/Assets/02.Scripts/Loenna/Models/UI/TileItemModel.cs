using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

public record TileItemModel(
	int LayerIndex,
	LocationType Location,
	Guid Guid,
	int Icon,
	Vector2 Position,
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
		int goldPuzzleCount
	) : this(
		layerIndex,
		location,
		guid,
		icon,
		position,
		goldPuzzleCount,
		new()
	)
	{

	}
}
