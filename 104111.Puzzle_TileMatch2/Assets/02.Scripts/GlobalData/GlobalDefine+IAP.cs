using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NineTap.Payment;

public static partial class GlobalDefine
{
    public static bool IsPurchasePending()
    {
        bool mainPending = globalData.mainScene?.m_purchasing.activeSelf ?? false;
        bool popupPending = globalData.storeScene?.m_purchasing.activeSelf ?? false;
        
        return mainPending || popupPending;
    }

    private static void StartActivityIndicator()
    {
        Debug.Log(CodeManager.GetMethodName());

        globalData.mainScene?.m_purchasing.SetActive(true);
        globalData.storeScene?.m_purchasing.SetActive(true);

        ActivityIndicatorManager.StartActivityIndicator();
    }

    private static void StopActivityIndicator()
    {
        Debug.Log(CodeManager.GetMethodName());

        globalData.mainScene?.m_purchasing.SetActive(false);
        globalData.storeScene?.m_purchasing.SetActive(false);

        ActivityIndicatorManager.StopActivityIndicator();
    }

    public static void Purchase(ProductData productData)
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
    }

    public static void ShowIAPResult_Success(ProductData productData, UnityEngine.Purchasing.Product product, IPaymentResult.Success result)
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
        globalData.ShowPresentPopup(productData);
    }

    public static void ShowIAPResult_Fail(ProductData productData, UnityEngine.Purchasing.Product product, IPaymentResult.Error error)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : ERROR : {1}", productData.ProductId, error.ToString()));

        StopActivityIndicator();
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