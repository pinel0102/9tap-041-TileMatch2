using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static LevelEditor;

public static partial class TileSearch
{
    private const int needTargetCount_Bush = 2;
    private const int needTargetCount_Chain = 2;

    public static Tile GetRandomTile(this List<Tile> tileList)
    {
        return tileList.OrderBy(g => Guid.NewGuid()).ToList()[0];
    }

    public static List<Tile> GetRandomTiles(this List<Tile> tileList, int count)
    {
        return tileList.OrderBy(g => Guid.NewGuid()).Take(count).ToList();
    }

    public static List<Tile> GetBlockerAvailableTiles(this Layer layer, BlockerType blockerType)
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
    /// [에디터] 타일에 블로커 설치 가능 여부.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <param name="blockerType"></param>
    /// <returns></returns>
    private static bool CanSetupBlocker(this Tile tile, Layer layer, BlockerType blockerType)
    {
        if (tile.BlockerType != BlockerType.None)
            return false;
        
        switch(blockerType)
        {
            case BlockerType.Glue_Left: // 오른쪽 타일이 일반 타일일 때 설치 가능.
                (bool existGlueLeftTarget, var tileGlueLeftTarget) = tile.FindRightTile(layer);
                if (existGlueLeftTarget && tileGlueLeftTarget.BlockerType is BlockerType.None)
                    return true;
                break;

            case BlockerType.Glue_Right: // Glue_Left가 설치된 곳의 오른쪽에만 설치 가능.
                (bool existGlueRightTarget, var tileGlueRightTarget) = tile.FindLeftTile(layer);
                if (existGlueRightTarget && tileGlueRightTarget.BlockerType is BlockerType.Glue_Left)
                    return true;
                break;

            case BlockerType.Bush: // 상하좌우에 일반 타일이 2개 이상일 때 설치 가능.
                var tileBushTargetList = tile.FindAroundTiles(layer);
                if (tileBushTargetList.Count >= needTargetCount_Bush)
                    return tileBushTargetList.Where(item => item.BlockerType is BlockerType.None or BlockerType.Jelly).Count() >= needTargetCount_Bush;
                break;

            case BlockerType.Suitcase: // 아래에 타일이 없을 때 설치 가능.
                var (existSuitcaseBottom, _) = tile.FindBottomTile(layer);
                if (!existSuitcaseBottom)
                    return true;
                break;

            case BlockerType.Jelly: // 항상 설치 가능. (일반 타일과 동일 취급.)
                return true;
                
            case BlockerType.Chain: // 좌우에 일반 타일이 2개일 때 설치 가능.
                var tileChainTargetList = tile.FindLeftRightTiles(layer);
                if (tileChainTargetList.Count >= needTargetCount_Chain)
                    return tileChainTargetList.Where(item => item.BlockerType is BlockerType.None or BlockerType.Jelly).Count() >= needTargetCount_Chain;
                break;

            default:
                break;
        }

        return false;
    }

    /// <summary>
    /// [에디터] 클리어 가능한 Blocker인지 체크.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="layer"></param>
    /// <param name="blockerType"></param>
    /// <returns></returns>
    public static bool IsValidBlocker(this TileInfo tile, LayerInfo layer, BlockerType blockerType, Action onNonValid = null)
    {
        if (tile.BlockerType == BlockerType.None)
            return true;

        bool isValid = false;
        
        switch(blockerType)
        {
            case BlockerType.Glue_Left: // 오른쪽 타일이 Glue_Right일 때 클리어 가능.
                (bool existGlueLeftTarget, var tileGlueLeftTarget) = tile.FindRightTile(layer);
                if (existGlueLeftTarget && tileGlueLeftTarget.BlockerType is BlockerType.Glue_Right)
                    isValid = true;
                break;

            case BlockerType.Glue_Right: // 왼쪽 타일이 Glue_Left일 때 클리어 가능.
                (bool existGlueRightTarget, var tileGlueRightTarget) = tile.FindLeftTile(layer);
                if (existGlueRightTarget && tileGlueRightTarget.BlockerType is BlockerType.Glue_Left)
                    isValid = true;
                break;

            case BlockerType.Bush: // 상하좌우에 일반 타일이 2개 이상일 때 클리어 가능.
                var tileBushTargetList = tile.FindAroundTiles(layer);
                if (tileBushTargetList.Count >= needTargetCount_Bush)
                    isValid = tileBushTargetList.Where(item => item.BlockerType is BlockerType.None or BlockerType.Jelly).Count() >= needTargetCount_Bush;
                break;

            case BlockerType.Suitcase: // 아래에 타일이 없을 때 클리어 가능.
                var (existSuitcaseBottom, _) = tile.FindBottomTile(layer);
                if (!existSuitcaseBottom)
                    isValid = true;
                break;

            case BlockerType.Jelly: // 항상 클리어 가능. (일반 타일과 동일 취급.)
                isValid = true;
                break;
                
            case BlockerType.Chain: // 좌우에 일반 타일이 2개일 때 클리어 가능.
                var tileChainTargetList = tile.FindLeftRightTiles(layer);
                if (tileChainTargetList.Count >= needTargetCount_Chain)
                    isValid = tileChainTargetList.Where(item => item.BlockerType is BlockerType.None or BlockerType.Jelly).Count() >= needTargetCount_Chain;
                break;

            default:
                break;
        }

        if(!isValid)
            onNonValid?.Invoke();

        return isValid;
    }
}
