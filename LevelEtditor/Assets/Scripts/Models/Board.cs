#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Board
{
	public int NumberOfTileTypes;
	public List<Layer> Layers;
	
	public Board(IEnumerable<Layer> layers)
	{
		NumberOfTileTypes = 1;
		Layers = new(layers);
	}

	public Layer? this[int index] => Layers.ElementAtOrDefault(index);
}
