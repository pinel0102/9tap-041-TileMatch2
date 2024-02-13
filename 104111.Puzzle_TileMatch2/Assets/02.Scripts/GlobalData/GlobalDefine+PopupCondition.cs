using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static partial class GlobalDefine
{
    public static bool IsEnable_BeginnerBundle()
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>testAutoPopupEditor : {0}</color>", testAutoPopupEditor));
            return true;
        }
#endif

        bool openCheck = IsOpen_BeginnerBundle();
        bool notShowedCheck = !globalData.userManager.Current.ShowedPopupBeginner;
        
        return openCheck && notShowedCheck;
    }

    public static bool IsEnable_Weekend1Bundle()
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>testAutoPopupEditor : {0}</color>", testAutoPopupEditor));
            return true;
        }
#endif

        CheckWeekendExpired();

        bool weekendCheck = IsWeekend();
        bool openCheck = IsOpen_Weekend1Bundle();
        bool notShowedCheck = !globalData.userManager.Current.ShowedPopupWeekend1;
        
        //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} / {1} / {2}</color>", weekendCheck, openCheck, notShowedCheck));
        
        return weekendCheck && openCheck && notShowedCheck;
    }

    public static bool IsEnable_Weekend2Bundle()
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>testAutoPopupEditor : {0}</color>", testAutoPopupEditor));
            return true;
        }
#endif

        CheckWeekendExpired();

        bool weekendCheck = IsWeekend();
        bool openCheck = IsOpen_Weekend2Bundle();
        bool notShowedCheck = !globalData.userManager.Current.ShowedPopupWeekend2;
        bool prevBundleCheck = globalData.userManager.Current.PurchasedWeekend1;

        //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} / {1} / {2} / {3}</color>", weekendCheck, openCheck, notShowedCheck, prevBundleCheck));
        
        return weekendCheck && openCheck && notShowedCheck && prevBundleCheck;
    }




    public static bool IsEnable_HardBundle()
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>testAutoPopupEditor : {0}</color>", testAutoPopupEditor));
            return true;
        }
#endif

        bool levelCheck = IsOpen_DailyRewards();
        if (!levelCheck) 
            return false;

        bool difficultyCheck = false;

        CheckDailyRewardExpired();
        
        DateTime lastTime = ToDateTime(globalData.userManager.Current.DailyRewardDate);
        TimeSpan ts = DateTime.Now.Subtract(lastTime);
        bool dateCheck = ts.TotalDays > 0;

        //if (levelCheck && dateCheck)
        //    Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1} -> {2}</color>", levelCheck && dateCheck, lastTime, DateTime.Now));
        
        return levelCheck && difficultyCheck && dateCheck;
    }

    public static bool IsEnable_CheerUp()
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>testAutoPopupEditor : {0}</color>", testAutoPopupEditor));
            return true;
        }
#endif

        bool levelCheck = IsOpen_DailyRewards();
        if (!levelCheck) 
            return false;

        CheckDailyRewardExpired();
        
        DateTime lastTime = ToDateTime(globalData.userManager.Current.DailyRewardDate);
        TimeSpan ts = DateTime.Now.Subtract(lastTime);
        bool dateCheck = ts.TotalDays > 0;

        //if (levelCheck && dateCheck)
        //    Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1} -> {2}</color>", levelCheck && dateCheck, lastTime, DateTime.Now));
        
        return levelCheck && dateCheck;
    }


    private static void CheckWeekendExpired()
    {
        if (IsExpired(ToDateTime(globalData.userManager.Current.WeekendEndDate)))
        {
            DateTime nextWeekendStart = GetNextWeekendStart();

            globalData.userManager.UpdateBundle(
                ShowedPopupWeekend1: false,
                ShowedPopupWeekend2: false,
                PurchasedWeekend1: false,
                PurchasedWeekend2: false,                
                WeekendStartDate: nextWeekendStart.ToString(dateFormat_HHmmss),
                WeekendEndDate: nextWeekendStart.AddDays(2).ToString(dateFormat_HHmmss)
            );

            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Expired] Next Weekend : {0} ~ {1}</color>", globalData.userManager.Current.WeekendStartDate, globalData.userManager.Current.WeekendEndDate));
        }
    }

    /*private static bool IsExpired(string startDate, int days)
    {
        DateTime lastTime = ToDateTime(startDate);
        TimeSpan ts = DateTime.Now.Subtract(lastTime);
        bool expired = ts.TotalDays > days;

        return expired;
    }*/
}