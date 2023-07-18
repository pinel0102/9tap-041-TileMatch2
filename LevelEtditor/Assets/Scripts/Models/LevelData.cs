#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public record LevelData(
	Guid Guid,
	int Key,
	List<Board> Boards,
	int TileCount,
	int Difficult = (int)DifficultType.NORMAL
)
{
	[JsonConstructor]
	public LevelData(int key, List<Board> boards, int tileCount, int difficult)
		: this (Guid.NewGuid(), key, boards, tileCount, difficult)
	{
		
	}

	public static LevelData CreateData(int level)
	{
		List<Board> boards = new();
		List<Layer> layers = new();
		layers.Add(new Layer());
		boards.Add(new Board(layers));

		return new LevelData(level, boards, tileCount: 0, difficult: (int)DifficultType.NORMAL);
	}

	public Board? this[int index] => Boards.ElementAtOrDefault(index);
	public Layer? GetLayer(int boardIndex, int layerIndex) => this[boardIndex]?[layerIndex];
	public int TileCountAll => Boards.Sum(board => board.Layers.Sum(Layer => Layer.Tiles.Count()));
}
