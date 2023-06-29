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
		layers.Add(Layer.Init);
		boards.Add(new Board(layers));

		return new LevelData(level, boards, 0, 1);
	}

	public Board? this[int index] 
	{
		get
		{
			return Boards.HasIndex(index) switch {
				true => Boards[index],
				_ => default(Board)
			};
		}
	}

	public Layer? GetLayer(int boardIndex, int layerIndex) => this[boardIndex]?[layerIndex];
}
