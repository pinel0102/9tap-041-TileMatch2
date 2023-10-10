using System.Collections.Generic;
using System.Linq;


public class BoardItemModel
{
	public int Index;
	public int LayerCount;
	public List<TileItemModel> Tiles;

	public BoardItemModel(int index, int layerCount, List<TileItemModel> tiles)
	{
		Index = index;
		LayerCount = layerCount;
		Tiles = tiles;
	}

	public static BoardItemModel Empty = new BoardItemModel (index: -1, layerCount: 0, tiles: new());
    public int TileCount => Tiles.Count(tile => tile.Location is not LocationType.POOL);

}