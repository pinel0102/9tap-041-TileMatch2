using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NineTap.Payment;

public static partial class GlobalDefine
{
    public const int ProductIndex_Beginner = 20207;
    public const int ProductIndex_HardLevel = 20208;
    public const int ProductIndex_Cheerup1 = 20209;
    public const int ProductIndex_Cheerup2 = 20210;
    public const int ProductIndex_Weekend1 = 20211;
    public const int ProductIndex_Weekend2 = 20212;
    public const int ProductIndex_PiggyBank = 20301;
    public const int ProductIndex_ADBlock = 29901;
    public const int ProductIndex_DailyBonus = 30000;

    public static readonly TimeSpan bundleDelay_Hard_Purchased = new TimeSpan(3, 0, 0);
    public static readonly TimeSpan bundleDelay_Hard_Default = new TimeSpan(0, 15, 0);
    public static readonly TimeSpan bundleDelay_Cheerup_Purchased = new TimeSpan(6, 0, 0);
    public static readonly TimeSpan bundleDelay_Cheerup_Default = new TimeSpan(0, 15, 0);
    
    public static bool IsPurchasePending()
    {
        bool mainPending = globalData?.mainScene?.m_purchasing.activeSelf ?? false;
        bool popupPending = globalData?.storeScene?.m_purchasing.activeSelf ?? false;
        
        return mainPending || popupPending;
    }

    private static void StartActivityIndicator()
    {
        Debug.Log(CodeManager.GetMethodName());

        globalData?.mainScene?.m_purchasing.SetActive(true);
        globalData?.storeScene?.m_purchasing.SetActive(true);

        ActivityIndicatorManager.StartActivityIndicator();
    }

    private static void StopActivityIndicator()
    {
        Debug.Log(CodeManager.GetMethodName());

        globalData?.mainScene?.m_purchasing.SetActive(false);
        globalData?.storeScene?.m_purchasing.SetActive(false);

        ActivityIndicatorManager.StopActivityIndicator();
    }

    /*public static void Purchase(ProductData productData)
    {
        PaymentService paymentService = Game.Inst.Get<PaymentService>();

        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", productData.ProductId, productData.FullName));

        StartActivityIndicator();

        paymentService?.Request(
            productData.Index, 
            onSuccess: (product, result) => {
                ShowIAPResult_Success(productData, product, result);
            },
            onError: (product, result) => {
                ShowIAPResult_Fail(productData, product, result);
            }
        );
    }*/

    public static void Purchase(ProductData productData, Action onSuccess = null, Action onFailed = null)
    {
        PaymentService paymentService = Game.Inst.Get<PaymentService>();

        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", productData.ProductId, productData.FullName));

        StartActivityIndicator();

        paymentService?.Request(
            productData.Index, 
            onSuccess: (product, result) => {
                ShowIAPResult_Success(productData, product, result, onSuccess);
            },
            onError: (product, result) => {
                ShowIAPResult_Fail(productData, product, result, onFailed);
            }
        );
    }

    public static void ShowIAPResult_Success(ProductData productData, UnityEngine.Purchasing.Product product, IPaymentResult.Success result, Action onComplete = null)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : SUCCESS : {1}", productData.ProductId, result.ToString()));

        globalData.userManager.UpdateLog(totalPayment: globalData.userManager.Current.TotalPayment + Convert.ToSingle(product.metadata.localizedPrice));
        SDKManager.SendAnalytics_IAP_Purchase(product);

        StopActivityIndicator();

        // [TODO] IAP Result

        //product.Coin
        //product.Contents.GetValueOrDefault(key, 0)
        // [1] : Return
        // [2] : Undo
        // [3] : Shuffle
        // [4] : Infinity Heart (Min)

        Dictionary<SkillItemType, int> addSkillItems = new()
        {
            { SkillItemType.Stash,      (int)productData.Contents.GetValueOrDefault(1, 0) },
            { SkillItemType.Undo,       (int)productData.Contents.GetValueOrDefault(2, 0) },
            { SkillItemType.Shuffle,    (int)productData.Contents.GetValueOrDefault(3, 0) }
        };

        long addBooster = productData.Contents.GetValueOrDefault(4, 0);

        GetItems(productData.Coin, addSkillItems, addBooster);

        SetADFreeUser();

        globalData.ShowPresentPopup(productData, onComplete);
    }

    public static void ShowIAPResult_Fail(ProductData productData, UnityEngine.Purchasing.Product product, IPaymentResult.Error error, Action onComplete = null)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : ERROR : {1}", productData.ProductId, error.ToString()));

        StopActivityIndicator();

        onComplete?.Invoke();
    }

    private static void SetADFreeUser()
    {
        SDKManager.Instance.SetAdFreeUser(true);
        RequestAD_HideBanner();

        if(!GlobalData.Instance.userManager.Current.NoAD)
            GlobalData.Instance.userManager.UpdateLog(noAD: true);

        RefreshADFreeUI();
    }

    public static void RefreshADFreeUI()
    {
        globalData.fragmentStore.RefreshADFreeUI();
        globalData.fragmentStore_popup?.RefreshADFreeUI();
    }
}