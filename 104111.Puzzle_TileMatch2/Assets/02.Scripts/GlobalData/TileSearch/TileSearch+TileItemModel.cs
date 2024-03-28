using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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
    /// Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static List<TileItemModel> FindBottomTiles(this TileItemModel tile)
    {
        List<TileItemModel> result = new List<TileItemModel>();

        (bool existBottom,  List<TileItemModel> bottomTiles)    = tile.FindBottomTileList();

        if (existBottom) result.AddRange(bottomTiles);

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

    /// <summary>
    /// Bottom
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>(Exist ? Tile : null)</returns>
    public static (bool, List<TileItemModel>) FindBottomTileList(this TileItemModel tile)
    {
        return tile.FindTile(tile.PositionBottom()).AsList();
    }

    /// <summary>
    /// ICD를 변경시켜야 할 타일을 검색. (본인 제외)
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static List<TileItemModel> FindTilesToChangeICD(this TileItemModel tile)
    {
        List<TileItemModel> result = new List<TileItemModel>();
        
        result = GlobalData.Instance.playScene.TileItems
            .FindAll(tileItem => tileItem.Current.Location == LocationType.BOARD && tileItem.Current != tile && tileItem.IsInteractable &&
                tileItem.blockerType switch{
                    BlockerType.Bush => tileItem.Current.FindAroundTiles().Contains(tile),
                    BlockerType.Chain => tileItem.Current.FindLeftRightTiles().Contains(tile),
                    BlockerType.Jelly => true,
                    _ => false
                })
            .Select(tile => { return tile.Current; }).ToList();

        return result;
    }

    public static TileItem FindTileItem(this TileItemModel tile)
    {
        return GlobalData.Instance.playScene.TileItems.Find(tileItem => tileItem.Current == tile);
    }

    public static bool ContainsTileCount(this TileItemModel tile)
    {
        return tile.BlockerType != BlockerType.Suitcase;
    }

    public static Vector2 GetSuitcaseTilePosition(this TileItemModel tileItem, int offsetICD = 0, bool isInit = false)
    {
        var(existTop, topTile) = tileItem.FindTopTile();
        if (existTop)
        {
            bool isActivatedTile = tileItem.BlockerICD + offsetICD >= topTile.BlockerICD;

            if (isInit)
            {
                //Debug.Log(CodeManager.GetMethodName() + isActivatedTile);
                TileItem tile = tileItem.FindTileItem();
                tile.isActivatedSuitcaseTile = isActivatedTile;
                tile.RefreshBlockerState(tileItem.BlockerType, tileItem.BlockerICD);
                tile.SetInteractable(tileItem.Location, tileItem.Overlapped, tileItem.InvisibleIcon, false).Forget();
            }
            
            return isActivatedTile ?
                tileItem.Position + Constant.Game.SUITCASE_TILE_SHOW_POSITION:
                tileItem.Position + Constant.Game.SUITCASE_TILE_HIDE_POSITION;
        }

        return tileItem.Position;
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
                if (tile.BlockerType != BlockerType.Suitcase_Tile)
                    Debug.LogWarning(CodeManager.GetMethodName() + string.Format("[{0}] {1}", tile.Guid, tile.Position));
            });
        }

        return (list.Count > 0, list.Count > 0 ? list[0] : null);
    }

    private static (bool, List<TileItemModel>) AsList(this List<TileItemModel> list)
    {
        if (list.Count > 1)
        {
            list.ForEach(tile => {
                if (tile.BlockerType != BlockerType.Suitcase_Tile)
                    Debug.LogWarning(CodeManager.GetMethodName() + string.Format("[{0}] {1}", tile.Guid, tile.Position));
            });
        }

        return (list.Count > 0, list.Count > 0 ? list : null);
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
