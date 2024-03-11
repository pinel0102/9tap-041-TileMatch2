using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static partial class TileSearch
{
#region [Game] TileItemModel (88x92)

    /// <summary>
    /// Left / Right / Top / Bottom
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns>(Tiles List?) List</returns>
    public static List<List<TileItemModel>> FindAroundTiles(this List<TileItemModel> tiles)
    {
        List<List<TileItemModel>> result = new List<List<TileItemModel>>();

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
    public static List<TileItemModel> FindAroundTiles(this TileItemModel tile)
    {
        List<TileItemModel> result = new List<TileItemModel>();

        (bool existLeft,    TileItemModel leftTile)      = tile.FindLeftTile();
        (bool existRight,   TileItemModel rightTile)     = tile.FindRightTile();
        (bool existTop,     TileItemModel topTile)       = tile.FindTopTile();
        (bool existBottom,  TileItemModel bottomTile)    = tile.FindBottomTile();

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
    public static List<TileItemModel> FindLeftRightTiles(this TileItemModel tile)
    {
        List<TileItemModel> result = new List<TileItemModel>();

        (bool existLeft,    TileItemModel leftTile)      = tile.FindLeftTile();
        (bool existRight,   TileItemModel rightTile)     = tile.FindRightTile();

        if (existLeft) result.Add(leftTile);
        if (existRight) result.Add(rightTile);

        return result;
    }

    /// <summary>
    /// Top / Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>Existng Tiles List</returns>
    public static List<TileItemModel> FindTopBottomTiles(this TileItemModel tile)
    {
        List<TileItemModel> result = new List<TileItemModel>();

        (bool existTop,     TileItemModel topTile)       = tile.FindTopTile();
        (bool existBottom,  TileItemModel bottomTile)    = tile.FindBottomTile();

        if (existTop) result.Add(topTile);
        if (existBottom) result.Add(bottomTile);

        return result;
    }

    /// <summary>
    /// Left
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileItemModel) FindLeftTile(this TileItemModel tile)
    {
        return tile.FindTile(tile.PositionLeft()).ListCheck();
    }

    /// <summary>
    /// Right
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileItemModel) FindRightTile(this TileItemModel tile)
    {
        return tile.FindTile(tile.PositionRight()).ListCheck();
    }

    /// <summary>
    /// Top
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileItemModel) FindTopTile(this TileItemModel tile)
    {
        return tile.FindTile(tile.PositionTop()).ListCheck();
    }

    /// <summary>
    /// Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileItemModel) FindBottomTile(this TileItemModel tile)
    {
        return tile.FindTile(tile.PositionBottom()).ListCheck();
    }

#endregion [Game] TileItemModel


#region Internal Find

    private static List<TileItemModel> FindTile(this TileItemModel tile, Vector2 checkPosition)
    {
        return tile.FindTile(tile.GetLayerTiles(), checkPosition);
    }

    private static List<TileItemModel> FindTile(this TileItemModel tile, List<TileItemModel> layerTiles, Vector2 checkPosition)
    {
        return layerTiles.FindAll(target => target != tile && target.Position.Equals(checkPosition));
    }

    private static (bool, TileItemModel) ListCheck(this List<TileItemModel> list)
    {
        if (list.Count > 1)
        {
            list.ForEach(tile => {
                Debug.LogWarning(CodeManager.GetMethodName() + string.Format("[{0}] {1}", tile.Guid, tile.Position));
            });
        }

        return (list.Count > 0, list.Count > 0 ? list[0] : null);
    }

    private static List<TileItemModel> GetLayerTiles(this TileItemModel tile)
    {
        return GlobalData.Instance.playScene.TileItems
            .FindAll(tileItem => tileItem.Current.Location == LocationType.BOARD && tileItem.Current.LayerIndex == tile.LayerIndex).ToModel();
    }

    private static List<TileItemModel> ToModel(this List<TileItem> tileItems)
    {
        List<TileItemModel> result = new List<TileItemModel>();
        tileItems.ForEach(tile => result.Add(tile.Current));
        return result;
    }

#endregion Internal Find

}
