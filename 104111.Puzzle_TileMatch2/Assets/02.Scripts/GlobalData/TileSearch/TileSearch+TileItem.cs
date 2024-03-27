using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static partial class TileSearch
{
#region [Game] TileItem (88x92)

    /// <summary>
    /// Left / Right / Top / Bottom
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns>(Tiles List?) List</returns>
    public static List<List<TileItem>> FindAroundTiles(this List<TileItem> tiles)
    {
        List<List<TileItem>> result = new List<List<TileItem>>();

        tiles.ForEach(tile => {
            result.Add(tile.FindAroundTiles());
        });

        return result;
    }

    /// <summary>
    /// Left / Right / Top / Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>Existng Tiles List</returns>
    public static List<TileItem> FindAroundTiles(this TileItem tile)
    {
        List<TileItem> result = new List<TileItem>();

        (bool existLeft,    TileItem leftTile)      = tile.FindLeftTile();
        (bool existRight,   TileItem rightTile)     = tile.FindRightTile();
        (bool existTop,     TileItem topTile)       = tile.FindTopTile();
        (bool existBottom,  TileItem bottomTile)    = tile.FindBottomTile();

        if (existLeft) result.Add(leftTile);
        if (existRight) result.Add(rightTile);
        if (existTop) result.Add(topTile);
        if (existBottom) result.Add(bottomTile);

        return result;
    }

    /// <summary>
    /// Left / Right
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>Existng Tiles List</returns>
    public static List<TileItem> FindLeftRightTiles(this TileItem tile)
    {
        List<TileItem> result = new List<TileItem>();

        (bool existLeft,    TileItem leftTile)      = tile.FindLeftTile();
        (bool existRight,   TileItem rightTile)     = tile.FindRightTile();

        if (existLeft) result.Add(leftTile);
        if (existRight) result.Add(rightTile);

        return result;
    }

    /// <summary>
    /// Top / Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>Existng Tiles List</returns>
    public static List<TileItem> FindTopBottomTiles(this TileItem tile)
    {
        List<TileItem> result = new List<TileItem>();

        (bool existTop,     TileItem topTile)       = tile.FindTopTile();
        (bool existBottom,  TileItem bottomTile)    = tile.FindBottomTile();

        if (existTop) result.Add(topTile);
        if (existBottom) result.Add(bottomTile);

        return result;
    }

    /// <summary>
    /// Left
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileItem) FindLeftTile(this TileItem tile)
    {
        return tile.FindTile(tile.PositionLeft()).ListCheck();
    }

    /// <summary>
    /// Right
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileItem) FindRightTile(this TileItem tile)
    {
        return tile.FindTile(tile.PositionRight()).ListCheck();
    }

    /// <summary>
    /// Top
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileItem) FindTopTile(this TileItem tile)
    {
        return tile.FindTile(tile.PositionTop()).ListCheck();
    }

    /// <summary>
    /// Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileItem) FindBottomTile(this TileItem tile)
    {
        return tile.FindTile(tile.PositionBottom()).ListCheck();
    }

    /// <summary>
    /// ICD를 변경시켜야 할 타일을 검색. (본인 제외)
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static List<TileItem> FindTilesToChangeICD(this TileItem tile)
    {
        return GlobalData.Instance.playScene.TileItems
            .FindAll(tileItem => tileItem.Current.Location == LocationType.BOARD && tileItem != tile && tileItem.IsInteractable &&
                tileItem.blockerType switch{
                    BlockerType.Bush => tileItem.FindAroundTiles().Contains(tile),
                    BlockerType.Chain => tileItem.FindLeftRightTiles().Contains(tile),
                    BlockerType.Jelly => true,
                    _ => false
                });
    }

    public static bool ContainsTileCount(this TileItem tile)
    {
        return tile.blockerType != BlockerType.Suitcase;
    }

#endregion [Game] TileItem


#region Internal Find

    private static List<TileItem> FindTile(this TileItem tile, Vector2 checkPosition)
    {
        return tile.FindTile(tile.GetLayerTiles(), checkPosition);
    }

    private static List<TileItem> FindTile(this TileItem tile, List<TileItem> layerTiles, Vector2 checkPosition)
    {
        return layerTiles.FindAll(target => target != tile && target.Current.Position.Equals(checkPosition));
    }

    private static (bool, TileItem) ListCheck(this List<TileItem> list)
    {
        if (list.Count > 1)
        {
            list.ForEach(tile => {
                Debug.LogWarning(CodeManager.GetMethodName() + string.Format("[{0}] {1}", tile.Current.Guid, tile.Current.Position));
            });
        }

        return (list.Count > 0, list.Count > 0 ? list[0] : null);
    }

    private static List<TileItem> GetLayerTiles(this TileItem tile)
    {
        return GlobalData.Instance.playScene.TileItems
            .FindAll(tileItem => tileItem.Current.Location == LocationType.BOARD && tileItem.Current.LayerIndex == tile.Current.LayerIndex);
    }

#endregion Internal Find

}
