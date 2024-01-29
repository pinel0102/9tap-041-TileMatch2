using System;
using System.Collections;
using System.Collections.Generic;
using NineTap.Common;
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

    public static void GetItems(
        Optional<long> addCoin = default, 
        Optional<Dictionary<SkillItemType, int>> addSkillItems = default, 
        Optional<long> addBooster = default)
    {
        globalData.userManager.GetItems(
            addCoin: addCoin,
            addSkillItems: addSkillItems,
            addBooster: addBooster
        );
    }

    public static void GetItem_Life(int addCount)
    {
        if (addCount > 0)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Life] {0}</color>", addCount));
        
        globalData.userManager.GetItem_Life(addCount);
    }

    public static void AddRewards(Dictionary<ProductType, long> dict, List<IReward> rewards)
    {
        foreach (var reward in rewards)
        {
            if (!dict.ContainsKey(reward.Type))
            {
                dict.Add(reward.Type, 0);
            }
            dict[reward.Type] += reward.GetAmount();
        }
    }

    public static void UpdateRewards(Dictionary<ProductType, long> rewards)
    {
        globalData.userManager.UpdateRewards(rewards);
    }
}
