using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

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
                        ShowGoToStorePopup("Purchase Life", () => {
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

    // 하트 HUD 클릭시 하트 구매 팝업 열기.
    public void ShowBuyHeartPopup()
    {
        UIManager.ShowPopupUI<BuyHeartPopup>(
            new BuyHeartPopupParameter(
                Title: "More Lives",
                Message: "Time to next life:",
                ExitParameter: ExitBaseParameter.CancelParam,
                BaseButtonParameter: new UITextButtonParameter {
                    ButtonText = NineTap.Constant.Text.Button.REFILL,
                    OnClick = () => 
                    {
                        if (userManager.Current.Coin >= Constant.User.MAX_LIFE_COIN)
                        {
                            if (!userManager.Current.IsFullLife())
                            {
                                SDKManager.SendAnalytics_C_Scene(NineTap.Constant.Text.Button.REFILL);
                                
                                userManager.GetItems(
                                    addCoin: - Constant.User.MAX_LIFE_COIN
                                );

                                SDKManager.SendAnalytics_C_Item_Use("Coin", Constant.User.MAX_LIFE_COIN);

                                GlobalDefine.GetItem_Life(
                                    Constant.User.MAX_LIFE_COUNT - userManager.Current.Life
                                );

                                SDKManager.SendAnalytics_C_Item_Get("Life", Constant.User.MAX_LIFE_COUNT);
                            }
                        }
                        else
                        {
                            ShowGoToStorePopup("Purchase Coin", () => {
                                    SDKManager.SendAnalytics_C_Scene(NineTap.Constant.Text.Button.STORE);

                                    mainScene.scrollView.MoveTo((int)MainMenuType.STORE);
                                    UIManager.ClosePopupUI_ForceAll();
                                }
                            );
                        }
                    },
                    SubWidgetBuilder = () => {
                        var widget = Instantiate(ResourcePathAttribute.GetResource<IconWidget>());
							widget.OnSetup("UI_Icon_Coin", $"{Constant.User.MAX_LIFE_COIN}");
							return widget.CachedGameObject;
                    }
                },
                LifeStatus: HUD.MessageBroker.Subscribe().Select(user => user.GetLifeStatus())
            )
        );
    }

    // 재화 부족시 상점 열기.
    public void ShowGoToStorePopup(string message, Action onClick = null)
    {
        Debug.Log(CodeManager.GetMethodName());
        
        //[MainScene:PlayButton] 하트 부족 알림.
        //[MainScene:HUD:Puzzle] 하트 부족 알림.
        //[MainScene:HUD:Heart] 코인 부족 알림.
        UIManager.ShowPopupUI<GiveupPopup>(
            new GiveupPopupParameter(
                Title: "Purchase",
                Message: message,
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

    public void ShowDailyRewardPopup()
    {
        Debug.Log(CodeManager.GetMethodName());

        UIManager.ShowPopupUI<DailyRewardPopup>(
            new DailyRewardPopupParameter(
                userManager.Current.DailyRewardDate,
                userManager.Current.DailyRewardIndex,
                VisibleHUD: HUDType.ALL
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
        Debug.Log(CodeManager.GetMethodName());

        UmpManager.Show_Ump();
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

    public void ShowReviewPopup()
    {
        userManager.UpdateReviewPopup(
            reviewPopupCount: userManager.Current.ReviewPopupCount + 1
        );

        Debug.Log(CodeManager.GetMethodName() + string.Format("ReviewPopupCount : {0}", userManager.Current.ReviewPopupCount));

        UIManager.ShowPopupUI<ReviewPopup>(
            new ReviewPopupParameter(
                Title: NineTap.Constant.Text.Popup.Title.REVIEW,
                Message: NineTap.Constant.Text.Popup.Message.REVIEW_1,
                Message2: NineTap.Constant.Text.Popup.Message.REVIEW_2,
                ExitParameter: ExitBaseParameter.CancelParam,
                BaseButtonParameter: new UITextButtonParameter {
                    ButtonText = NineTap.Constant.Text.Button.SURE,
                    OnClick = SetRated
                },
                LeftButtonParameter: new UITextButtonParameter {
                    ButtonText = NineTap.Constant.Text.Button.NO_THANKS,
                    OnClick = UIManager.ClosePopupUI
                },
                CloseButtonParameter: new UITextButtonParameter {
                    ButtonText = NineTap.Constant.Text.Button.OK,
                    OnClick = UIManager.ClosePopupUI
                },
                HUDTypes: HUDType.ALL
            )
        );

        void SetRated()
        {            
            userManager.UpdateReviewPopup(
                isRated: true,
                reviewPopupCount: 1000,
                reviewPopupDate: DateTime.Today.AddYears(1000).ToString(GlobalDefine.dateFormat_HHmmss)
            );

            Debug.Log(CodeManager.GetMethodName() + string.Format("IsRated : {0}", userManager.Current.IsRated));
        }
    }
}