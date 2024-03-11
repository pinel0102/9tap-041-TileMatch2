using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LevelEditor;

public static partial class TileSearch
{
#region [Editor] TileInfo (160x160)

    /// <summary>
    /// Left / Right / Top / Bottom
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="layer"></param>
    /// <returns>(Tiles List?) List</returns>
    public static List<List<TileInfo>> FindAroundTiles(this List<TileInfo> tiles, LayerInfo layer)
    {
        List<List<TileInfo>> result = new List<List<TileInfo>>();

        tiles.ForEach(tile => {
            result.Add(tile.FindAroundTiles(layer));
        });

        return result;
    }

    /// <summary>
    /// Left / Right / Top / Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>Existng Tiles List</returns>
    public static List<TileInfo> FindAroundTiles(this TileInfo tile, LayerInfo layer)
    {
        List<TileInfo> result = new List<TileInfo>();

        (bool existLeft,    TileInfo leftTile)      = tile.FindLeftTile(layer);
        (bool existRight,   TileInfo rightTile)     = tile.FindRightTile(layer);
        (bool existTop,     TileInfo topTile)       = tile.FindTopTile(layer);
        (bool existBottom,  TileInfo bottomTile)    = tile.FindBottomTile(layer);

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
    /// <param name="layer"></param>
    /// <returns>Existng Tiles List</returns>
    public static List<TileInfo> FindLeftRightTiles(this TileInfo tile, LayerInfo layer)
    {
        List<TileInfo> result = new List<TileInfo>();

        (bool existLeft,    TileInfo leftTile)      = tile.FindLeftTile(layer);
        (bool existRight,   TileInfo rightTile)     = tile.FindRightTile(layer);

        if (existLeft) result.Add(leftTile);
        if (existRight) result.Add(rightTile);

        return result;
    }

    /// <summary>
    /// Top / Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>Existng Tiles List</returns>
    public static List<TileInfo> FindTopBottomTiles(this TileInfo tile, LayerInfo layer)
    {
        List<TileInfo> result = new List<TileInfo>();

        (bool existTop,     TileInfo topTile)       = tile.FindTopTile(layer);
        (bool existBottom,  TileInfo bottomTile)    = tile.FindBottomTile(layer);

        if (existTop) result.Add(topTile);
        if (existBottom) result.Add(bottomTile);

        return result;
    }

    /// <summary>
    /// Left
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileInfo) FindLeftTile(this TileInfo tile, LayerInfo layer)
    {
        return tile.FindTile(layer, tile.PositionLeft()).ListCheck();
    }

    /// <summary>
    /// Right
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileInfo) FindRightTile(this TileInfo tile, LayerInfo layer)
    {
        return tile.FindTile(layer, tile.PositionRight()).ListCheck();
    }

    /// <summary>
    /// Top
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileInfo) FindTopTile(this TileInfo tile, LayerInfo layer)
    {
        return tile.FindTile(layer, tile.PositionTop()).ListCheck();
    }

    /// <summary>
    /// Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, TileInfo) FindBottomTile(this TileInfo tile, LayerInfo layer)
    {
        return tile.FindTile(layer, tile.PositionBottom()).ListCheck();
    }

#endregion [Editor] Tile


#region Internal Find

    private static List<TileInfo> FindTile(this TileInfo tile, LayerInfo layer, Vector2 checkPosition)
    {
        return tile.FindTile(layer.Tiles.ToList(), checkPosition);
    }

    private static List<TileInfo> FindTile(this TileInfo tile, List<TileInfo> layerTiles, Vector2 checkPosition)
    {
        return layerTiles.FindAll(target => target != tile && target.Position.Equals(checkPosition));
    }
    
    private static (bool, TileInfo) ListCheck(this List<TileInfo> list)
    {
        if (list.Count > 1)
        {
            list.ForEach(tile => {
                Debug.LogWarning(CodeManager.GetMethodName() + string.Format("[{0}] {1}", tile.Guid, tile.Position));
            });
        }

        return (list.Count > 0, list.Count > 0 ? list[0] : null);
    }

#endregion Internal Find

}
