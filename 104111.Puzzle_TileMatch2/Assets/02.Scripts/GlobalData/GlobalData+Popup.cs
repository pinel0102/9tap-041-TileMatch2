using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public partial class GlobalData
{
    public void ShowStorePopup(Action onStoreClosed = null)
    {
        UIManager.backKeyCallback = onStoreClosed;

        UIManager.ShowSceneUI<StoreScene>(
            new StoreSceneParameter(
                StoreParam: new MainSceneFragmentContentParameter_Store {
                    TitleText = "Store",
                    CloseButtonParameter = new UIImageButtonParameter {
                        OnClick = () => { 
                            UIManager.ReturnBackUI(UIManager.backKeyCallback);
                        }
                    }
                }
            )
        );
    }

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
                        SDKManager.SendAnalytics_C_Scene(NineTap.Constant.Text.Button.STORE);
                        ShowBuyHeartPopup();
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
        SetTouchLock_MainScene(true);

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

                                //GlobalDefine.GetItem_Life(Constant.User.MAX_LIFE_COUNT - userManager.Current.Life);
                                GlobalDefine.ResetLife();

                                SDKManager.SendAnalytics_C_Item_Get("Life", Constant.User.MAX_LIFE_COUNT);
                            }
                        }
                        else
                        {
                            SDKManager.SendAnalytics_C_Scene(NineTap.Constant.Text.Button.STORE);
                            mainScene.scrollView.MoveTo((int)MainMenuType.STORE);
                            UIManager.ClosePopupUI_ForceAll();
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

    public async UniTask ShowDailyRewardPopup()
    {
        Debug.Log(CodeManager.GetMethodName());

        //SetTouchLock_MainScene(true);

        bool popupClosed = false;

        UIManager.ShowPopupUI<DailyRewardPopup>(
            new DailyRewardPopupParameter(
                userManager.Current.DailyRewardDate,
                userManager.Current.DailyRewardIndex,
                () => { popupClosed = true; },
                VisibleHUD: HUDType.ALL
            )
        );

        await UniTask.WaitUntil(() => popupClosed);

        //SetTouchLock_MainScene(false);
    }

    public void ShowPuzzleOpenPopup()
    {
        UIManager.ShowPopupUI<PuzzleOpenPopup>(
            new PuzzleOpenPopupParameter(
                OpenPuzzleIndex: userManager.Current.PuzzleOpenPopupIndex
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
                NewLandmark: 0,
                OnComplete: null,
                VisibleHUD: HUDType.NONE
            )
        );
    }

    public void ShowPresentPopup(RewardData rewardData, Action onComplete = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        UIManager.ShowPopupUI<RewardPopup>(
            new RewardPopupParameter (
                PopupType: RewardPopupType.PRESENT,
                Reward: rewardData,
                NewLandmark: 0,
                OnComplete: onComplete,
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

    public async UniTask ShowReviewPopup()
    {
        //SetTouchLock_MainScene(true);

        userManager.UpdateReviewPopup(
            reviewPopupCount: userManager.Current.ReviewPopupCount + 1
        );

        Debug.Log(CodeManager.GetMethodName() + string.Format("ReviewPopupCount : {0}", userManager.Current.ReviewPopupCount));

        bool popupClosed = false;

        UIManager.ShowPopupUI<ReviewPopup>(
            new ReviewPopupParameter(
                Title: NineTap.Constant.Text.Popup.Title.REVIEW,
                Message: NineTap.Constant.Text.Popup.Message.REVIEW_1,
                Message2: NineTap.Constant.Text.Popup.Message.REVIEW_2,
                ExitParameter: new ExitBaseParameter(() => popupClosed = true, false),
                BaseButtonParameter: new UITextButtonParameter {
                    ButtonText = NineTap.Constant.Text.Button.SURE,
                    OnClick = SetRated
                },
                LeftButtonParameter: new UITextButtonParameter {
                    ButtonText = NineTap.Constant.Text.Button.NO_THANKS,
                    OnClick = () => {
                        UIManager.ClosePopupUI_Force();
                        popupClosed = true;
                    }
                },
                CloseButtonParameter: new UITextButtonParameter {
                    ButtonText = NineTap.Constant.Text.Button.OK,
                    OnClick = () => {
                        UIManager.ClosePopupUI_Force();
                        popupClosed = true;
                    }
                },
                HUDTypes: HUDType.ALL
            )
        );

        await UniTask.WaitUntil(() => popupClosed);

        //SetTouchLock_MainScene(false);

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