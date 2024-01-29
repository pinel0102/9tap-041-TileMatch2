using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static partial class GlobalDefine
{
    public static void UpdateDailyReward(int dailyIndex)
    {
        globalData.userManager.UpdateDailyReward(
            dailyRewardDate: DateTime.Today.AddDays(1).ToString(dateFormat_HHmmss),
            dailyRewardIndex: dailyIndex);
    }

    public static bool IsEnableDailyRewards()
    {
        bool levelCheck = globalData.userManager.Current.Level >= Constant.User.LEVEL_DAILY_REWARD_START;
        if (!levelCheck) 
            return false;

        CheckDailyRewardExpired();
        
        DateTime lastTime = ToDateTime(globalData.userManager.Current.DailyRewardDate);
        TimeSpan ts = DateTime.Now.Subtract(lastTime);
        bool dateCheck = ts.TotalDays > 0;

        if (levelCheck && dateCheck)
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] {1} -> {2}</color>", levelCheck && dateCheck, lastTime, DateTime.Now));
        
        return levelCheck && dateCheck;
    }

    private static void CheckDailyRewardExpired()
    {
        if (IsExpiredDailyReward())
        {
            globalData.userManager.UpdateDailyReward(
                dailyRewardDate: DateTime.Today.ToString(dateFormat_HHmmss),
                dailyRewardIndex: 0);

            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Expired] Reset to {0}</color>", globalData.userManager.Current.DailyRewardDate));
        }
    }

    private static bool IsExpiredDailyReward()
    {
        DateTime lastTime = ToDateTime(globalData.userManager.Current.DailyRewardDate);
        TimeSpan ts = DateTime.Now.Subtract(lastTime);
        bool dateCheck = ts.TotalDays > 1;

        return dateCheck;
    }
}
