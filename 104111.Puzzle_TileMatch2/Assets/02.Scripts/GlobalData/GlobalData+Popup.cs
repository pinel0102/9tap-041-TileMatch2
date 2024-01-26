using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public partial class GlobalData
{
    public void ShowReadyPopup(Action onComplete = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        User user = userManager.Current;

        UIManager.ShowPopupUI<ReadyPopup>(
        new ReadyPopupParameter(
            Level: user.Level,
            ExitParameter: new ExitBaseParameter(
                onExit:()=>{
                    UIManager.ClosePopupUI_ForceAll();
                }
            ),//ExitBaseParameter.CancelParam,
            BaseButtonParameter: new UITextButtonParameter{
                ButtonText = NineTap.Constant.Text.Button.PLAY,
                OnClick = () => 
                {
                    var (_, valid, _) = user.Valid();
                    if (!valid)
                    {
                        ShowHeartBuyPopup(() => {
                            SDKManager.SendAnalytics_C_Scene(NineTap.Constant.Text.Button.STORE);
                            mainScene.scrollView.MoveTo((int)MainMenuType.STORE);}
                        );
                        return;
                    }
                    
                    SDKManager.SendAnalytics_C_Scene(NineTap.Constant.Text.Button.PLAY);
                    UIManager.ShowSceneUI<PlayScene>(new PlaySceneParameter());
                }
            },
            AllPressToClose: true,
            HUDTypes: HUDType.ALL,
            OnComplete: onComplete
        ));
    }

    public void ShowHeartBuyPopup(Action onClick = null)
    {
        Debug.Log(CodeManager.GetMethodName());
        
        //[MainScene:PlayButton] 하트 부족 알림.
        //[MainScene:HUD:Puzzle] 하트 부족 알림.
        UIManager.ShowPopupUI<GiveupPopup>(
            new GiveupPopupParameter(
                Title: "Purchase",
                Message: "Purchase Life",
                ignoreBackKey: false,
                ExitParameter: ExitBaseParameter.CancelParam,
                BaseButtonParameter: new UITextButtonParameter {
                    ButtonText = "Go to Shop",
                    OnClick = onClick
                },
                HUDTypes: HUDType.ALL
            )
        );
    }

    public void ShowRemoveAdsPopup(Action onClick = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        /*UIManager.ShowPopupUI<GiveupPopup>(
            new GiveupPopupParameter(
                Title: "Purchase",
                Message: "Purchase Life",
                ignoreBackKey: false,
                ExitParameter: ExitBaseParameter.CancelParam,
                BaseButtonParameter: new UITextButtonParameter {
                    ButtonText = "Go to Shop",
                    OnClick = onClick
                },
                HUDTypes: HUDType.ALL
            )
        );*/
    }

    public void ShowPresentPopup(ProductData product)
    {
        Debug.Log(CodeManager.GetMethodName());

        UIManager.ShowPopupUI<RewardPopup>(
            new RewardPopupParameter (
                PopupType: RewardPopupType.PRESENT,
                Reward: product.ToRewardData(),
                VisibleHUD: HUDType.NONE
            )
        );
    }

    public void ShowPresentPopup(RewardData rewardData)
    {
        Debug.Log(CodeManager.GetMethodName());

        UIManager.ShowPopupUI<RewardPopup>(
            new RewardPopupParameter (
                PopupType: RewardPopupType.PRESENT,
                Reward: rewardData,
                VisibleHUD: HUDType.NONE
            )
        );
    }

    public void ShowAgreePopup_GDPR(string region)
    {
        Debug.Log(CodeManager.GetMethodName());

        UIManager.ShowPopupUI<AgreeGDPRPopup>(
            new AgreeGDPRPopupParameter(
                Region: region
            )
        );
    }

    public void ShowAgreePopup_CMP(string region)
    {
#if CMP_CHECK_ENABLE

        Debug.Log(CodeManager.GetMethodName());

#if UNITY_EDITOR
        //userManager.UpdateAgree(agreeCMP: true);
        return;
#else
        string CMPString = PlayerPrefs.GetString("IABTCF_AddtlConsent");
        
        Debug.Log(CodeManager.GetMethodName() + string.Format("CMPString : {0}", CMPString));
        if (CMPString.Contains("2878"))
        {
            Debug.Log(CodeManager.GetMethodName() + "setConsent : True");
            //IronSource.Agent.setConsent(true);
        }
        else
        {
            Debug.Log(CodeManager.GetMethodName() + "setConsent : False");
            //IronSource.Agent.setConsent(false);
        }
#endif

#endif
    }

    public void ShowAgreePopup_ATT(string region)
    {
#if (UNITY_IOS && !UNITY_STANDALONE) || UNITY_EDITOR
        Debug.Log(CodeManager.GetMethodName());

        UIManager.ShowPopupUI<AgreeATTPopup>(
            new AgreeATTPopupParameter(
                Region: region
            )
        );
#endif
    }
}