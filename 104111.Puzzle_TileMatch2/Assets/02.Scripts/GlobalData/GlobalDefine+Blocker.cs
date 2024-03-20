using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static partial class GlobalDefine
{
    public static bool isBlockerTutorialTest = false;

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

    public static string GetBlockerName(BlockerType blockerType)
    {
        return blockerType switch{
            BlockerType.Glue_Left or BlockerType.Glue_Right => "Glue",
            BlockerType.Bush => "Bush",
            BlockerType.Suitcase => "Suitcase",
            BlockerType.Jelly => "Jelly",
            BlockerType.Chain => "Chain",
            _ => ""
        };
    }

    public static int RequiredBasketSpace(BlockerType blockerType)
    {
        return blockerType switch 
        {
            BlockerType.Glue_Left or BlockerType.Glue_Right => 2,
            _ => 1
        };
    }

#region Blocker Resource

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blockerType"></param>
    /// <param name="blockerICD"></param>
    /// <returns>Exist, if Exist => Path</returns>
    public static (bool, string) GetBlockerFXPrefabPath(BlockerType blockerType, int blockerICD)
    {
        string prefabName = blockerType switch
        {
            BlockerType.Glue_Right => "FX_Glue",
            BlockerType.Chain => "FX_Chain",
            BlockerType.Bush => "FX_Bush",
            BlockerType.Jelly => blockerICD switch
            {
                >= 3 => "FX_Jelly_01",
                2 => "FX_Jelly_02",
                1 => "FX_Jelly_03",
                _ => string.Empty
            },
            BlockerType.Suitcase => string.Empty,
            _ => string.Empty
        };

        bool exist = !string.IsNullOrEmpty(prefabName);
        string resultPath = exist ? string.Format(Format_FX_Prefab_Blocker, prefabName) : null;

        return (exist, resultPath);
    }

    public static string GetBlockerFXSoundPath(BlockerType blockerType, int blockerICD)
    {
        return blockerType switch
        {
            BlockerType.Glue_Left or BlockerType.Glue_Right => string.Empty,
            BlockerType.Chain => string.Empty,
            BlockerType.Bush => blockerICD switch
            {
                >= 2 => string.Empty,
                1 => string.Empty,
                _ => string.Empty
            },
            BlockerType.Jelly => blockerICD switch
            {
                >= 3 => string.Empty,
                2 => string.Empty,
                1 => string.Empty,
                _ => string.Empty
            },
            BlockerType.Suitcase => string.Empty,
            _ => string.Empty
        };
    }

#endregion Blocker Resource


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


#region Blocker Glue FX

    public const int GlueFX_Count = 8;
    public const float GlueFX_Count_Inverse = 0.125f; // 1/GlueFX_Count
    public const float GlueFX_Duration = 0.5f;

    public static (Vector3, Vector3) GluePathLast(Vector3 originLeft, Vector3 originRight)
    {
        return (originLeft + glueLeftPath.Last(), originRight + glueRightPath.Last());
    }

    public static (Vector3, Vector3) GlueRotationLast()
    {
        return (glueLeftRotation.Last(), glueRightRotation.Last());
    }

    public static (Vector3[], Vector3[]) GluePathArray(Vector3 originLeft, Vector3 originRight)
    {
        Vector3[] leftPath = new Vector3[GlueFX_Count] 
        {
            originLeft + glueLeftPath[0],
            originLeft + glueLeftPath[1],
            originLeft + glueLeftPath[2],
            originLeft + glueLeftPath[3],
            originLeft + glueLeftPath[4],
            originLeft + glueLeftPath[5],
            originLeft + glueLeftPath[6],
            originLeft + glueLeftPath[7],
        };

        Vector3[] rightPath = new Vector3[GlueFX_Count] 
        {
            originRight + glueRightPath[0],
            originRight + glueRightPath[1],
            originRight + glueRightPath[2],
            originRight + glueRightPath[3],
            originRight + glueRightPath[4],
            originRight + glueRightPath[5],
            originRight + glueRightPath[6],
            originRight + glueRightPath[7],
        };

        return (leftPath, rightPath);
    }

    public static (Vector3[], Vector3[]) GlueRotationArray()
    {
        return (glueLeftRotation, glueRightRotation);
    }

    private static readonly Vector3[] glueLeftPath = new Vector3[GlueFX_Count] 
    {
        new (0, 0, 0),      //0
        new (-2, 0, 0),     //1
        new (-4, 0, 0),     //2
        new (-6, 0, 0),     //3
        new (-20, -20, 0),  //4
        new (-30, -30, 0),  //5
        new (-40, -40, 0),  //6
        new (-50, -50, 0),  //7
    };

    private static readonly Vector3[] glueRightPath = new Vector3[GlueFX_Count] 
    {
        new (0, 0, 0),     //0
        new (2, 0, 0),     //1
        new (4, 0, 0),     //2
        new (6, 0, 0),     //3
        new (20, -20, 0),  //4
        new (30, -30, 0),  //5
        new (40, -40, 0),  //6
        new (50, -50, 0),  //7
    };

    private static readonly Vector3[] glueLeftRotation = new Vector3[GlueFX_Count] 
    {
        new (0, 0, 0),   //0
        new (0, 0, 2),   //1
        new (0, 0, 4),   //2
        new (0, 0, 6),   //3
        new (0, 0, 16),  //4
        new (0, 0, 16),  //5
        new (0, 0, 30),  //6
        new (0, 0, 30),  //7
    };

    private static readonly Vector3[] glueRightRotation = new Vector3[GlueFX_Count] 
    {
        new (0, 0, 0),   //0
        new (0, 0, -2),  //1
        new (0, 0, -4),  //2
        new (0, 0, -6),  //3
        new (0, 0, -16), //4
        new (0, 0, -16), //5
        new (0, 0, -25), //6
        new (0, 0, -30), //7
    };

#endregion Blocker Glue FX
}
