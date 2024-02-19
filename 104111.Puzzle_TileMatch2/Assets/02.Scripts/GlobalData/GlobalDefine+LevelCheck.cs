using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static partial class GlobalDefine
{
    /// <Summary>
    /// Required Level &amp; Not Purchased
    /// </Summary>
    public static bool IsOpen_BeginnerBundle()
    {
        bool notPurchasedCheck = !globalData.userManager.Current.PurchasedBeginner;
        return globalData.userManager.Current.Level >= Constant.User.MIN_OPENLEVEL_BEGINNER_BUNDLE && notPurchasedCheck;
    }

    /// <Summary>
    /// Required Level &amp; Not Purchased
    /// </Summary>
    public static bool IsOpen_Weekend1Bundle()
    {
        bool notPurchasedCheck = !globalData.userManager.Current.PurchasedWeekend1;
        return globalData.userManager.Current.Level >= Constant.User.MIN_OPENLEVEL_WEEKEND_BUNDLE && notPurchasedCheck;
    }

    /// <Summary>
    /// Required Level &amp; Not Purchased
    /// </Summary>
    public static bool IsOpen_Weekend2Bundle()
    {
        bool notPurchasedCheck = !globalData.userManager.Current.PurchasedWeekend2;
        return globalData.userManager.Current.Level >= Constant.User.MIN_OPENLEVEL_WEEKEND_BUNDLE && notPurchasedCheck;
    }

    /// <Summary>
    /// Required Level
    /// </Summary>
    public static bool IsOpen_DailyRewards()
    {
        return globalData.userManager.Current.Level >= Constant.User.MIN_OPENLEVEL_DAILY_REWARD;
    }

    /// <Summary>
    /// Required Level
    /// </Summary>
    public static bool IsOpen_Event_SweetHolic()
    {
        return globalData.userManager.Current.Level >= Constant.User.MIN_OPENLEVEL_EVENT_SWEETHOLIC;
    }
}