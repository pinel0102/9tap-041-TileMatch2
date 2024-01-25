using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static partial class GlobalDefine
{
    public const string SCENE_MAIN = "Main";
    public const string SCENE_COLLECTION = "Collection";
    public const string SCENE_PUZZLE = "Puzzle";
    public const string SCENE_STORE = "Store";
    public const string SCENE_SETTINGS = "Settings";
    public const string SCENE_PLAY = "Play";

    private static GlobalData globalData { get { return GlobalData.Instance; } }

    public static string GetNewUserGroup()
    {
        string group = "A";

        /*int rnd = Random.Range(0, 100);
        switch(rnd)
        {
            case < 50: group = "B"; break;
            default: group = "A"; break;
        }*/

        return group;
    }

    public static void GetItems(int addCoin, Dictionary<SkillItemType, int> addItems, long addBooster)
    {
        if (addCoin > 0)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Coin] {0}</color>", addCoin));
        foreach(var item in addItems)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1}</color>", item.Key, item.Value));
        if (addBooster > 0)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Booster] {0}m</color>", addBooster));

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Before Booster] {0} / ({1})</color>", globalData.userManager.Current.ExpiredLifeBoosterAt, globalData.userManager.Current.ExpiredLifeBoosterTime));

        globalData.userManager.GetItems(
            addCoin: addCoin,
            addSkillItems: addItems,
            addBooster: addBooster
        );

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[After Booster] {0} / ({1})</color>", globalData.userManager.Current.ExpiredLifeBoosterAt, globalData.userManager.Current.ExpiredLifeBoosterTime));
    }

    public static void GetItem_Life(int addCount)
    {
        if (addCount > 0)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Life] {0}</color>", addCount));
        
        globalData.userManager.GetItem_Life(addCount);
    }
}
