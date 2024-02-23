using System;
using System.Collections;
using System.Collections.Generic;
using NineTap.Common;
using UnityEngine;

public static partial class GlobalDefine
{
    private static bool testAutoPopupEditor = false;

    public const string SCENE_MAIN = "Main";
    public const string SCENE_COLLECTION = "Collection";
    public const string SCENE_PUZZLE = "Puzzle";
    public const string SCENE_STORE = "Store";
    public const string SCENE_SETTINGS = "Settings";
    public const string SCENE_PLAY = "Play";

    private static GlobalData globalData { get { return GlobalData.Instance; } }

    public static void Initialize()
    {
        InitRandomSeed();

        CheckEventActivate();
        CheckEventRefresh();
    }

    public static void InitRandomSeed()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
    }

    public static void CheckEventActivate()
    {
        Debug.Log(CodeManager.GetMethodName());

        if(globalData?.userManager?.Current != null)
        {
            globalData.eventSweetHolic_Activate = IsOpen_Event_SweetHolic() && !IsExpired(ToDateTime(globalData.userManager.Current.Event_SweetHolic_EndDate));
        }
    }

    public static void CheckEventRefresh()
    {
        Debug.Log(CodeManager.GetMethodName());
        
        SweetHolic_RefreshTarget();
    }

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

    public static void ResetLife()
    {
        Debug.Log(CodeManager.GetMethodName());

        globalData.userManager.ResetLife();
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

    public static bool IsEnablePuzzleOpenPopup()
    {
        return globalData.userManager.Current.PuzzleOpenPopupIndex > 0;
    }

    public static UnityEngine.Pool.IObjectPool<T> ResetObjectPool<T>(this UnityEngine.Pool.IObjectPool<T> objectPool, Transform parent, Func<T> createFunc, Action<T> actionOnRelease) where T : MonoBehaviour
    {
        for(int i = parent?.childCount - 1 ?? -1; i >= 0; i--)
        {
            GameObject.Destroy(parent?.GetChild(i).gameObject);
        }

        return new UnityEngine.Pool.ObjectPool<T>(
			createFunc: createFunc,
			actionOnRelease: actionOnRelease
		);
    }
}
