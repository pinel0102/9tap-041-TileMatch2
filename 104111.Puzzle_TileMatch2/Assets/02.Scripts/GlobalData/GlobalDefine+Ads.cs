using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static partial class GlobalDefine
{
#region 광고

    public static void RequestAD_ShowBanner()
    {
        Debug.Log(CodeManager.GetMethodName());

        SDKManager.Instance.ShowBanner();
    }

    public static void RequestAD_HideBanner()
    {
        Debug.Log(CodeManager.GetMethodName());

        UniTask.Void(
            async () => {
                for(int i=0; i < 5; i++)
                {
                    SDKManager.Instance.HideBanner();
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                }
            }
        );
    }

    public static void RequestAD_Interstitial(bool openRemoveAdsPopup = true)
    {
        Debug.Log(CodeManager.GetMethodName());
        
        SDKManager.Instance.ShowInterstitial(openRemoveAdsPopup);
    }

    public static void RequestAD_RewardVideo(int num)
    {
        Debug.Log(CodeManager.GetMethodName() + num);

        SDKManager.Instance.ShowRewardVideo(num);
    }

    public static void GetReward_FromVideo(int num)
    {
        Debug.Log(CodeManager.GetMethodName() + num);

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