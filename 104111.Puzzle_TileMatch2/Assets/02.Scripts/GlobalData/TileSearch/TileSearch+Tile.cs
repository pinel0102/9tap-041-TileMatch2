using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static partial class TileSearch
{
#region [Editor] Tile (160x160)

    /// <summary>
    /// Left / Right / Top / Bottom
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="layer"></param>
    /// <returns>(Tiles List?) List</returns>
    public static List<List<Tile>> FindAroundTiles(this List<Tile> tiles, Layer layer)
    {
        List<List<Tile>> result = new List<List<Tile>>();

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
    public static List<Tile> FindAroundTiles(this Tile tile, Layer layer)
    {
        List<Tile> result = new List<Tile>();

        (bool existLeft,    Tile leftTile)      = tile.FindLeftTile(layer);
        (bool existRight,   Tile rightTile)     = tile.FindRightTile(layer);
        (bool existTop,     Tile topTile)       = tile.FindTopTile(layer);
        (bool existBottom,  Tile bottomTile)    = tile.FindBottomTile(layer);

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
    public static List<Tile> FindLeftRightTiles(this Tile tile, Layer layer)
    {
        List<Tile> result = new List<Tile>();

        (bool existLeft,    Tile leftTile)      = tile.FindLeftTile(layer);
        (bool existRight,   Tile rightTile)     = tile.FindRightTile(layer);

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
    public static List<Tile> FindTopBottomTiles(this Tile tile, Layer layer)
    {
        List<Tile> result = new List<Tile>();

        (bool existTop,     Tile topTile)       = tile.FindTopTile(layer);
        (bool existBottom,  Tile bottomTile)    = tile.FindBottomTile(layer);

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
    public static (bool, Tile) FindLeftTile(this Tile tile, Layer layer)
    {
        return tile.FindTile(layer, tile.PositionLeft()).ListCheck();
    }

    /// <summary>
    /// Right
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, Tile) FindRightTile(this Tile tile, Layer layer)
    {
        return tile.FindTile(layer, tile.PositionRight()).ListCheck();
    }

    /// <summary>
    /// Top
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, Tile) FindTopTile(this Tile tile, Layer layer)
    {
        return tile.FindTile(layer, tile.PositionTop()).ListCheck();
    }

    /// <summary>
    /// Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, Tile) FindBottomTile(this Tile tile, Layer layer)
    {
        return tile.FindTile(layer, tile.PositionBottom()).ListCheck();
    }

#endregion [Editor] Tile


#region Internal Find

    private static List<Tile> FindTile(this Tile tile, Layer layer, Vector2 checkPosition)
    {
        return tile.FindTile(layer.Tiles, checkPosition);
    }

    private static List<Tile> FindTile(this Tile tile, List<Tile> layerTiles, Vector2 checkPosition)
    {
        return layerTiles.FindAll(target => target != tile && target.Position.Equals(checkPosition));
    }
    
    private static (bool, Tile) ListCheck(this List<Tile> list)
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
