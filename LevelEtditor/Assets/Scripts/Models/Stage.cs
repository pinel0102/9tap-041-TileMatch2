#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public record LevelData(
	int Level,
	List<Board> Boards,
	int TileCount,
	int NumberOfTileTypes
)
{
	public Layer? GetLayer(int boardIndex, int layerIndex) => Boards?.ElementAtOrDefault(boardIndex)?.Layers?.ElementAtOrDefault(layerIndex);
}
