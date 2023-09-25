using System;

public class PlaySceneBoardViewParameter
{
	public TileItem TilePrefab;
	public int NumberOfTileTypes;
	public Action<(int layerIndex, TileItem tile)> OnClickTile;
}
