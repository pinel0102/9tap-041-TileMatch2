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

    public static void RequestAD_RewardVideo(int num)
    {
        Debug.Log(string.Format(logAdFormat1, CodeManager.GetMethodName(), num));

        SDKManager.Instance.ShowRewardVideo(num);
    }

    public static void GetReward_FromVideo(int num)
    {
        Debug.Log(string.Format(logAdFormat1, CodeManager.GetMethodName(), num));

        switch(num)
        {
            case 0: 
                GetItems(addBooster:1);
                UIManager.ClosePopupUI_ForceAll();                
                break;
        }
    }

#endregion
}