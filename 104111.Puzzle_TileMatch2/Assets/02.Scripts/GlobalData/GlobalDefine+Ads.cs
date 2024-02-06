using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static partial class GlobalDefine
{
#region 광고

    private const string logAdColor = "#FF64FF";
    private readonly static string logAdFormat0 = $"<color={logAdColor}>{{0}}</color>";
    private readonly static string logAdFormat1 = $"<color={logAdColor}>{{0}} : {{1}}</color>";

    public static bool IsRewardVideoReady()
    {
        return SDKManager.IsRewardVideoLoaded;
    }

    public static void RequestAD_ShowBanner()
    {
        Debug.Log(string.Format(logAdFormat0, CodeManager.GetMethodName()));
        
        SDKManager.Instance.ShowBanner();
    }

    public static void RequestAD_HideBanner()
    {
        Debug.Log(string.Format(logAdFormat0, CodeManager.GetMethodName()));
        
        SDKManager.Instance.HideBanner();
    }

    public static void RequestAD_Interstitial(bool openRemoveAdsPopup = true)
    {
        Debug.Log(string.Format(logAdFormat0, CodeManager.GetMethodName()));
        
        SDKManager.Instance.ShowInterstitial(openRemoveAdsPopup);
    }

    public static void RequestAD_RewardVideo(int num, Action<bool> onSuccess = null)
    {
        Debug.Log(string.Format(logAdFormat1, CodeManager.GetMethodName(), num));

        SDKManager.Instance.ShowRewardVideo(num, onSuccess);
    }

    public static bool IsEnableRemoveAdsPopup()
    {
#if UNITY_EDITOR
        if(testAutoPopupEditor)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>testAutoPopupEditor : {0}</color>", testAutoPopupEditor));
            return true;
        }
#endif
        
        bool userCheck = !globalData.userManager.Current.NoAD;
        if (!userCheck)
            return false;

        bool sdkCheck = !SDKManager.Instance.IsAdFreeUser && SDKManager.OpenRemoveAdsPopup;
        if (!sdkCheck)
            return false;

        CheckRemoveAdsPopupExpired();

        bool countCheck = globalData.userManager.Current.RemoveAdsPopupCount < 1;
        if (!countCheck)
            return false;
        
        DateTime lastTime = ToDateTime(globalData.userManager.Current.RemoveAdsPopupDate);
        TimeSpan ts = DateTime.Now.Subtract(lastTime);
        bool dateCheck = ts.TotalDays > 0;

        return userCheck && sdkCheck && countCheck && dateCheck;
    }

    private static void CheckRemoveAdsPopupExpired()
    {
        if (IsExpiredRemoveAdsPopup())
        {
            globalData.userManager.UpdateRemoveAdsPopup(
                RemoveAdsPopupCount: 0,
                RemoveAdsPopupDate: DateTime.Today.ToString(dateFormat_HHmmss)
            );

            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[Expired] Reset to {0}</color>", globalData.userManager.Current.RemoveAdsPopupDate));
        }
    }

    private static bool IsExpiredRemoveAdsPopup()
    {
        DateTime lastTime = ToDateTime(globalData.userManager.Current.RemoveAdsPopupDate);
        TimeSpan ts = DateTime.Now.Subtract(lastTime);
        bool dateCheck = ts.TotalDays > 1;

        return dateCheck;
    }

#endregion
}