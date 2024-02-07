using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static partial class GlobalDefine
{
    public static bool IsEnable_Review(int level)
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
            return true;
#endif

        // 레이트 여부 체크.
        if(globalData.userManager.Current.IsRated)
            return false;

        // 하루 2번 체크.
        CheckReviewPopupExpired();

        if(globalData.userManager.Current.ReviewPopupCount > 1)
            return false;

        // 레벨 체크.
        bool canShow = false;

        switch(level)
        {
            case 10:
            case 26:
                canShow = true;
                break;
            case > 30:
                canShow = level % 20 == 0;
                break;
        }

        return canShow;
    }

    private static void CheckReviewPopupExpired()
    {
        if (IsExpiredReviewPopup())
        {
            globalData.userManager.UpdateReviewPopup(
                reviewPopupCount: 0,
                reviewPopupDate: DateTime.Today.ToString(dateFormat_HHmmss)
            );

            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Expired] Last ReviewPopup : {0}</color>", globalData.userManager.Current.ReviewPopupDate));
        }
    }

    private static bool IsExpiredReviewPopup()
    {
        DateTime lastTime = ToDateTime(globalData.userManager.Current.ReviewPopupDate);
        TimeSpan ts = DateTime.Now.Subtract(lastTime);
        bool dateCheck = ts.TotalDays > 1;

        return dateCheck;
    }
}
