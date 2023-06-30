using System;
using System.Collections.Generic;

[Serializable]
public class Layer
{
	public List<Tile> Tiles;

	public Layer()
	{
		Tiles = new();
	}
}
