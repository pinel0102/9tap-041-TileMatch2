using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public partial class GlobalData
{
    /// <summary>
    /// 첫 등장 : 10 클리어 (메인 / 자동)
    /// 조건 만족시 자동 등장 반복.
    /// </summary>
    public async UniTask ShowPopup_Review()
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

    /// <summary>
    /// 첫 등장 : 15 클리어 (메인 / 자동)
    /// 조건 만족시 자동 등장 반복.
    /// </summary>
    public async UniTask ShowPopup_DailyRewards()
    {
        Debug.Log(CodeManager.GetMethodName());

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
    }

    /// <summary>
    /// 첫 등장 : 20 클리어 + 광고 (메인 / 자동)
    /// 조건 만족시 자동 등장 반복.
    /// </summary>
    public async UniTask ShowPopup_RemoveAds()
    {
        Debug.Log(CodeManager.GetMethodName() + userManager.Current.RemoveAdsPopupCount);

        SDKManager.SetRemoveAdsPopup(false);

        if (tableManager.ProductDataTable.TryGetValue(GlobalDefine.ProductIndex_ADBlock, out var product))
        {
            bool popupClosed = false;

            userManager.UpdateRemoveAdsPopup(
                RemoveAdsPopupCount: userManager.Current.RemoveAdsPopupCount + 1
            );

            UIManager.ShowPopupUI<RemoveAdsPopup>(
                new RemoveAdsPopupParameter(
                    Title: $"{product.FullName}",
                    Message: $"{product.Description}",                    
                    PopupCloseCallback: () => { popupClosed = true; },
                    Product: product,
                    VisibleHUD: HUDType.ALL
                )
            );

            await UniTask.WaitUntil(() => popupClosed);
        }
    }
}