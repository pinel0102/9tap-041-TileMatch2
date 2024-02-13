using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public static partial class GlobalDefine
{
    /// <Summary>
    /// <para>Not Showed Popup</para>
    /// Required Level &amp; Not Purchased
    /// </Summary>
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

    /// <Summary>
    /// <para>Weekend &amp; Not Showed Popup</para>
    /// <para>Required Level &amp; Not Purchased</para>
    /// </Summary>
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

    /// <Summary>
    /// <para>Weekend &amp; Not Showed Popup &amp; Purchased Weekend 1</para>
    /// <para>Required Level &amp; Not Purchased</para>
    /// </Summary>
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

    /// <Summary>
    /// <para>Hard Level &amp; PopupDate Expired</para>
    /// </Summary>
    public static bool IsEnable_HardBundle()
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>testAutoPopupEditor : {0}</color>", testAutoPopupEditor));
            return true;
        }
#endif

        bool difficultyCheck = GlobalData.Instance.CURRENT_DIFFICULTY > 0;
        if (!difficultyCheck) 
            return false;

        bool dateCheck = IsExpired(ToDateTime(globalData.userManager.Current.NextPopupDateHard));
        
        if(!dateCheck)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] Next Popup : {1}</color>", dateCheck, globalData.userManager.Current.NextPopupDateHard));
        
        return difficultyCheck && dateCheck;
    }

    /// <Summary>
    /// <para>PopupDate Expired</para>
    /// </Summary>
    public static bool IsEnable_CheerUp()
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>testAutoPopupEditor : {0}</color>", testAutoPopupEditor));
            return true;
        }
#endif

        bool dateCheck = IsExpired(ToDateTime(globalData.userManager.Current.NextPopupDateCheerup));

        if (!dateCheck)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] Next Popup : {1}</color>", dateCheck, globalData.userManager.Current.NextPopupDateCheerup));
        
        return dateCheck;
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
}