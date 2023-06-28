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
	public static LevelData CreateData(int level)
	{
		List<Board> boards = new();
		List<Layer> layers = new();
		layers.Add(new Layer(0, new()));
		boards.Add(new Board(0, layers));

		return new LevelData(level, boards, 0, 1);
	}
	public Layer? GetLayer(int boardIndex, int layerIndex) => Boards?.ElementAtOrDefault(boardIndex)?.Layers?.ElementAtOrDefault(layerIndex);
}
