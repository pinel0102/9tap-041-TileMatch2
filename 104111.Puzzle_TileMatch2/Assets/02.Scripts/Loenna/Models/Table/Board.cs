#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Board
{
	public int NumberOfTileTypes;
	public List<Layer> Layers;
	public int Difficult;
	public int MissionTileCount;
	public int GoldTileIcon;
	
	public Board(IEnumerable<Layer> layers)
	{
		NumberOfTileTypes = 1;
		Layers = new(layers);
		Difficult = (int)DifficultType.NORMAL;
		GoldTileIcon = -1;
		MissionTileCount = 0;
	}

	public Layer? this[int index] => Layers.ElementAtOrDefault(index);
}
