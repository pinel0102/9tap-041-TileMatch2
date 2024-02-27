using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;
using Cysharp.Threading.Tasks;

public partial class GlobalData
{
    private async UniTask ShowProductPopup(ProductData productData, string backgroundImage, string ribbonImage, string coinImage, Action onPurchased = null)
    {
        bool popupClosed = false;

        UIManager.ShowPopupUI<ProductPopup>(
        new ProductPopupParameter(
            Title: productData.FullName,
            Message: productData.Description,
            ExitParameter: new ExitBaseParameter(CloseCallback, false),
            ProductData: productData,
            BackgroundImage: backgroundImage,
            RibbonImage: ribbonImage,
            CoinImage: coinImage,
            PurchasedCallback: onPurchased,
            PopupCloseCallback: CloseCallback,
            HUDTypes: CURRENT_SCENE == GlobalDefine.SCENE_PLAY ? HUDType.NONE : HUDType.ALL
        ));

        await UniTask.WaitUntil(() => popupClosed);

        void CloseCallback()
        {
            popupClosed = true;
        }
    }

    /// <summary>
    /// 등장 1 : 9 클리어 (메인 / 자동)
    /// 조건 만족시 자동 등장 (반복 X).
    /// </summary>
    public async UniTask ShowPopup_Beginner(Action onPurchased = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        if (tableManager.ProductDataTable.TryGetValue(GlobalDefine.ProductIndex_Beginner, out var productData))
        {
            userManager.UpdateBundle(
                ShowedPopupBeginner: true,
                LastPopupDateBeginner: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss)
            );

            await ShowProductPopup(productData, "Beginner", "Beginnertitle", "01", OnPurchased);
        }

        void OnPurchased()
        {
            userManager.UpdateBundle(
                PurchasedBeginner: true,
                PurchasedDateBeginner: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss)
            );

            onPurchased?.Invoke();
        }
    }

    /// <summary>
    /// 첫 등장 : 1 클리어 + 주말 (메인 / 자동)
    /// 조건 만족시 자동 등장 반복.
    /// </summary>
    public async UniTask ShowPopup_Weekend1(Action onPurchased = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        if (tableManager.ProductDataTable.TryGetValue(GlobalDefine.ProductIndex_Weekend1, out var productData))
        {
            userManager.UpdateBundle(
                ShowedPopupWeekend1: true,
                LastPopupDateWeekend1: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss)
            );

            await ShowProductPopup(productData, "Weekend", "Weekendtitle", "03", OnPurchased);
        }

        void OnPurchased()
        {
            userManager.UpdateBundle(
                PurchasedWeekend1: true,
                PurchasedDateWeekend1: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss)
            );

            onPurchased?.Invoke();
        }
    }

    /// <summary>
    /// 첫 등장 : 1 클리어 + 주말 + Weekend1 구매 (메인 / 자동)
    /// 조건 만족시 자동 등장 반복.
    /// </summary>
    public async UniTask ShowPopup_Weekend2(Action onPurchased = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        if (tableManager.ProductDataTable.TryGetValue(GlobalDefine.ProductIndex_Weekend2, out var productData))
        {
            userManager.UpdateBundle(
                ShowedPopupWeekend2: true,
                LastPopupDateWeekend2: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss)
            );

            await ShowProductPopup(productData, "Weekend", "Weekendtitle", "04", OnPurchased);
        }

        void OnPurchased()
        {
            userManager.UpdateBundle(
                PurchasedWeekend2: true,
                PurchasedDateWeekend2: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss)
            );

            onPurchased?.Invoke();
        }
    }

    /// <summary>
    /// 첫 등장 : 하드 레벨 시작시 (메인 / 자동)
    /// 조건 만족시 자동 등장 반복.
    /// </summary>
    public async UniTask ShowPopup_HardLevel(Action onPurchased = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        if (tableManager.ProductDataTable.TryGetValue(GlobalDefine.ProductIndex_HardLevel, out var productData))
        {
            userManager.UpdateBundle(
                PurchasedHard: false,
                LastPopupDateHard: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss),
                NextPopupDateHard: DateTime.Now.Add(GlobalDefine.bundleDelay_Hard_Default).ToString(GlobalDefine.dateFormat_HHmmss)
            );

            await ShowProductPopup(productData, "Hard", string.Empty, "02", OnPurchased);

            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] Next Popup : {1}</color>", userManager.Current.PurchasedHard, userManager.Current.NextPopupDateHard));
        }

        void OnPurchased()
        {
            userManager.UpdateBundle(
                PurchasedHard: true,
                PurchasedDateHard: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss),
                NextPopupDateHard: DateTime.Now.Add(GlobalDefine.bundleDelay_Hard_Purchased).ToString(GlobalDefine.dateFormat_HHmmss)
            );

            onPurchased?.Invoke();
        }
    }

    /// <summary>
    /// 첫 등장 : 레벨 실패시 (게임 / 자동)
    /// 조건 만족시 자동 등장 반복.
    /// </summary>
    public async UniTask ShowPopup_Cheerup1(Action onPurchased = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        if (tableManager.ProductDataTable.TryGetValue(GlobalDefine.ProductIndex_Cheerup1, out var productData))
        {
            userManager.UpdateBundle(
                PurchasedCheerup1: false,
                PurchasedCheerup2: false,
                LastPopupDateCheerup1: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss),
                NextPopupDateCheerup: DateTime.Now.Add(GlobalDefine.bundleDelay_Cheerup_Default).ToString(GlobalDefine.dateFormat_HHmmss)
            );

            await ShowProductPopup(productData, "Cheerup_01", string.Empty, "02", OnPurchased);

            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] Next Popup : {1}</color>", userManager.Current.PurchasedCheerup1, userManager.Current.NextPopupDateCheerup));
        }

        void OnPurchased()
        {
            userManager.UpdateBundle(
                PurchasedCheerup1: true,
                PurchasedCheerup2: false,
                PurchasedDateCheerup1: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss),
                NextPopupDateCheerup: DateTime.Now.Add(GlobalDefine.bundleDelay_Cheerup_Purchased).ToString(GlobalDefine.dateFormat_HHmmss)
            );

            onPurchased?.Invoke();
        }
    }

    /// <summary>
    /// 첫 등장 : 레벨 실패시 (게임 / 자동)
    /// 조건 만족시 자동 등장 반복.
    /// </summary>
    public async UniTask ShowPopup_Cheerup2(Action onPurchased = null)
    {
        Debug.Log(CodeManager.GetMethodName());

        if (tableManager.ProductDataTable.TryGetValue(GlobalDefine.ProductIndex_Cheerup2, out var productData))
        {
            userManager.UpdateBundle(
                PurchasedCheerup1: false,
                PurchasedCheerup2: false,
                LastPopupDateCheerup2: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss),
                NextPopupDateCheerup: DateTime.Now.Add(GlobalDefine.bundleDelay_Cheerup_Default).ToString(GlobalDefine.dateFormat_HHmmss)
            );

            await ShowProductPopup(productData, "Cheerup_02", string.Empty, "03", OnPurchased);

            Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[{0}] Next Popup : {1}</color>", userManager.Current.PurchasedCheerup2, userManager.Current.NextPopupDateCheerup));
        }

        void OnPurchased()
        {
            userManager.UpdateBundle(
                PurchasedCheerup1: false,
                PurchasedCheerup2: true,
                PurchasedDateCheerup2: DateTime.Now.ToString(GlobalDefine.dateFormat_HHmmss),
                NextPopupDateCheerup: DateTime.Now.Add(GlobalDefine.bundleDelay_Cheerup_Purchased).ToString(GlobalDefine.dateFormat_HHmmss)
            );

            onPurchased?.Invoke();
        }
    }
}