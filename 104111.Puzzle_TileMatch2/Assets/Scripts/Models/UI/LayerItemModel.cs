using System;
using System.Collections.Generic;
using System.Linq;


public class LayerItemModel
{
	public int Index;
	public List<TileItemModel> Tiles;

	public LayerItemModel(int index, List<TileItemModel> tiles)
    {
        Index = index;
        Tiles = tiles;
    }

    public void Deconstruct(out int index, out List<TileItemModel> tiles)
    {
        index = Index;
        tiles = Tiles;
    }
}