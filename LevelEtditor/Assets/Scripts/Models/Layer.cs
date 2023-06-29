using System;
using System.Collections.Generic;

[Serializable]
public record Layer(
	List<Tile> Tiles
)
{
	public static Layer Init = new Layer(Tiles: new List<Tile>());
	public string GetName(int index) => $"Layer {index}";
}
