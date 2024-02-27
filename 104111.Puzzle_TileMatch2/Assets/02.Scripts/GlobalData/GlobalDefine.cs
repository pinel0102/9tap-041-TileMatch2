using System;
using System.Collections;
using System.Collections.Generic;
using NineTap.Common;
using UnityEngine;

public static partial class GlobalDefine
{
    private static bool testAutoPopupEditor = false;

    public static bool IsUserLoaded { get; private set; }
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
        CheckEventExpired();
        CheckEventRefresh();
    }

    public static void SetUserLoaded(bool userLoaded)
    {
        IsUserLoaded = userLoaded;
    }

    public static void InitRandomSeed()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
    }

    /// <summary>
    /// <para>이벤트 오픈 체크.</para>
    /// <para>메인 씬으로 돌아올 때 1회 체크한다.</para>
    /// </summary>
    public static void CheckEventActivate()
    {
        if(IsUserLoaded)
        {
            Debug.Log(CodeManager.GetMethodName());

            globalData.eventSweetHolic_Activate = IsOpen_Event_SweetHolic() && !IsExpired(ToDateTime(globalData.userManager.Current.Event_SweetHolic_EndDate));
        }
    }

    /// <summary>
    /// <para>이벤트 만료 체크. (초마다 체크.)</para>
    /// <para>PlayScene 또는 AutoPopups 중에는 체크하지 않는다.</para>
    /// </summary>
    public static void CheckEventExpired()
    {
        if(IsUserLoaded && globalData.CURRENT_SCENE != SCENE_PLAY && !globalData.isAutoPopupPending)
        {
            //Debug.Log(CodeManager.GetMethodName());

            CheckSweetHolicExpired();
        }
    }

    /// <summary>
    /// <para>이벤트 상태 갱신.</para>
    /// <para>이벤트가 변경되었을 경우 새로운 내용으로 갱신한다.</para>
    /// </summary>
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
        ClearChild(parent);

        return new UnityEngine.Pool.ObjectPool<T>(
			createFunc: createFunc,
			actionOnRelease: actionOnRelease
		);
    }

    public static void ClearChild(Transform _transform)
    {
        for(int i = _transform?.childCount - 1 ?? -1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(_transform?.GetChild(i).gameObject);
        }
    }
}
