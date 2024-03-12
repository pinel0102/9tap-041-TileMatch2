using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static partial class GlobalDefine
{
    public static readonly Dictionary<BlockerTypeEditor, int> BlockerICD_Default = new Dictionary<BlockerTypeEditor, int>()
    {
        [BlockerTypeEditor.None] = 0,
        [BlockerTypeEditor.Glue] = 0,
        [BlockerTypeEditor.Bush] = 2,
        [BlockerTypeEditor.Suitcase] = 1,
        [BlockerTypeEditor.Jelly] = 3,
        [BlockerTypeEditor.Chain] = 1,
    };

    public static readonly Dictionary<BlockerType, BlockerTypeEditor> BlockerTransDictionary = new Dictionary<BlockerType, BlockerTypeEditor>()
    {
        [BlockerType.None] = BlockerTypeEditor.None,
        [BlockerType.Glue_Left] = BlockerTypeEditor.Glue,
        [BlockerType.Glue_Right] = BlockerTypeEditor.Glue,
        [BlockerType.Bush] = BlockerTypeEditor.Bush,
        [BlockerType.Suitcase] = BlockerTypeEditor.Suitcase,
        [BlockerType.Jelly] = BlockerTypeEditor.Jelly,
        [BlockerType.Chain] = BlockerTypeEditor.Chain,
    };

    public static int RequiredBasketSpace(BlockerType blockerType)
    {
        return blockerType switch 
        {
            BlockerType.Glue_Left or BlockerType.Glue_Right => 2,
            _ => 1
        };
    }

#region Blocker Sprite

    public static Sprite GetBlockerSprite(BlockerType blockerType, int blockerICD)
    {
        string path = null;

        switch(blockerType)
        {
            case BlockerType.Glue_Right:
                path = "Blocker_Glue_01";
                break;
            case BlockerType.Bush:
                if (blockerICD >= 2)
                    path = "Blocker_Bush_01";
                else if (blockerICD == 1)
                    path = "Blocker_Bush_02";
                break;
            case BlockerType.Suitcase:
                path = "Blocker_Suitcase_02";
                break;
            case BlockerType.Jelly:
                if (blockerICD >= 3)
                    path = "Blocker_Jelly_01";
                else if (blockerICD == 2)
                    path = "Blocker_Jelly_02";
                else if (blockerICD == 1)
                    path = "Blocker_Jelly_03";
                break;
            case BlockerType.Chain:
                path = "Blocker_Chain_01";
                break;
            case BlockerType.Glue_Left:
            case BlockerType.None:
            default:
                break;
        }

        return SpriteManager.GetSprite(path);
    }

    public static Sprite GetBlockerSubSprite(BlockerType blockerType, int blockerICD)
    {
        string path = null;

        switch(blockerType)
        {
            case BlockerType.Suitcase:
                path = "Blocker_Suitcase_01";
                break;
            default:
                break;
        }

        return SpriteManager.GetSprite(path);
    }

#endregion Blocker Sprite


#region Blocker ICD

    /// <summary>
    /// <para>가변 ICD를 사용하는 Blocker : _blockerICD (최소 1).</para>
    /// <para>가변 ICD를 사용하지 않는 Blocker : 고정값 (Max ICD).</para>
    /// </summary>
    /// <param name="_blockerType"></param>
    /// <param name="_blockerICD"></param>
    /// <returns></returns>
    public static int GetBlockerICD(BlockerType _blockerType, int _blockerICD)
    {
        if (TryParseBlockerType(_blockerType, out BlockerTypeEditor _blockerTypeEditor))
        {
            return GetBlockerICD(_blockerTypeEditor, _blockerICD);
        }
        
        return 0;
    }

    /// <summary>
    /// <para>가변 ICD를 사용하는 Blocker : _blockerICD (최소 1).</para>
    /// <para>가변 ICD를 사용하지 않는 Blocker : 고정값 (Max ICD).</para>
    /// </summary>
    /// <param name="_blockerType"></param>
    /// <param name="_blockerICD"></param>
    /// <returns></returns>
    public static int GetBlockerICD(BlockerTypeEditor _blockerType, int _blockerICD)
    {
        if (IsBlockerICD_Variable(_blockerType))
            return Mathf.Max(1, _blockerICD);
        else 
            return BlockerICD_Default[_blockerType];
    }

    public static bool IsBlockerICD_Variable(BlockerType _blockerType)
    {
        if (TryParseBlockerType(_blockerType, out BlockerTypeEditor _blockerTypeEditor))
        {
            return IsBlockerICD_Variable(_blockerTypeEditor);
        }
        
        return false;
    }

    public static bool IsBlockerICD_Variable(BlockerTypeEditor _blockerType)
    {
        switch(_blockerType)
        {
            case BlockerTypeEditor.Suitcase:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// [Suitcase] 추가 타일 카운트.
    /// </summary>
    /// <param name="layers"></param>
    /// <returns></returns>
    public static int GetAdditionalTileCount(List<Layer> layers)
    {
        return layers.Sum(layer => {
            return layer?.Tiles?
                .Where(tile => tile.BlockerType == BlockerType.Suitcase)
                .Sum(tile => { return Mathf.Max(0, tile.BlockerICD - 1); }) ?? 0;
        });
    }

#endregion Blocker ICD


#region Blocker Translate

    /// <summary>
    /// <para>BlockerTypeEditor -> BlockerType 변환.</para>
    /// <para>Glue는 Glue_Left, Glue_Right로 나뉘기 때문에 별도 작업 필요.</para>
    /// </summary>
    /// <param name="blockerTypeEditor"></param>
    /// <param name="blockerTypeGame"></param>
    /// <returns></returns>
    public static bool TryParseBlockerType(BlockerTypeEditor blockerTypeEditor, out List<BlockerType> blockerTypeGame)
    {
        blockerTypeGame = new List<BlockerType>();

        if(BlockerTransDictionary.ContainsValue(blockerTypeEditor))
        {
            var keyList = BlockerTransDictionary.Where(item => item.Value.Equals(blockerTypeEditor)).ToList();
            blockerTypeGame.AddRange(keyList.Select(item => item.Key));
            return true;
        }

        return false;
    }

    /// <summary>
    /// <para>BlockerType -> BlockerTypeEditor 변환.</para>
    /// <para>Glue_Left, Glue_Right는 Glue로 통합됨.</para>
    /// </summary>
    /// <param name="blockerTypeGame"></param>
    /// <param name="blockerTypeEditor"></param>
    /// <returns></returns>
    public static bool TryParseBlockerType(BlockerType blockerTypeGame, out BlockerTypeEditor blockerTypeEditor)
    {
        blockerTypeEditor = BlockerTypeEditor.None;
        
        if(BlockerTransDictionary.ContainsKey(blockerTypeGame))
        {
            blockerTypeEditor = BlockerTransDictionary[blockerTypeGame];
            return true;
        }

        return false;
    }

#endregion Blocker Translate
}
