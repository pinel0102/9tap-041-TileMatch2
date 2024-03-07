using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static partial class TileSearch
{
    public static List<Tile> GetBlockerEnableTiles(this Layer layer, BlockerType blockerType)
    {
        List<Tile> result = new List<Tile>();

        layer.Tiles.ForEach(tile => {
            if(tile.CanSetupBlocker(layer, blockerType))
            {
                result.Add(tile);
            }
        });

        return result;
    }

    /// <summary>
    /// 타일에 블로커 설치 가능 여부.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <param name="blockerType"></param>
    /// <returns></returns>
    private static bool CanSetupBlocker(this Tile tile, Layer layer, BlockerType blockerType)
    {
        if (tile.Blocker != BlockerType.None)
            return false;
        
        switch(blockerType)
        {
            case BlockerType.Bush: // 상하좌우에 일반 타일이 2개 이상일 때 설치 가능.
                int needCountBushTarget = 2;
                var tileBushTargetList = tile.FindAroundTiles(layer);
                if (tileBushTargetList.Count >= needCountBushTarget)
                    return tileBushTargetList.Where(item => item.Blocker is BlockerType.None).Count() >= needCountBushTarget;
                break;

            case BlockerType.Glue_Left: // 오른쪽 타일이 일반 타일일 때 설치 가능.
                (bool existGlueLeftTarget, var tileGlueLeftTarget) = tile.FindRightTile(layer);
                if (existGlueLeftTarget && tileGlueLeftTarget.Blocker is BlockerType.None)
                    return true;
                break;

            case BlockerType.Glue_Right: // Glue_Left가 설치된 곳의 오른쪽에만 설치 가능.
                (bool existGlueRightTarget, var tileGlueRightTarget) = tile.FindLeftTile(layer);
                if (existGlueRightTarget && tileGlueRightTarget.Blocker is BlockerType.Glue_Left)
                    return true;
                break;

            default:
                break;
        }
        return false;
    }
}
