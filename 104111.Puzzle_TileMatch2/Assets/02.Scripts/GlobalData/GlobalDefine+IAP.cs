using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NineTap.Payment;

public static partial class GlobalDefine
{
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

    public static void Purchase(ProductData product)
    {
        PaymentService paymentService = Game.Inst.Get<PaymentService>();

        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", product.ProductId, product.FullName));

        StartActivityIndicator();

#if UNITY_EDITOR
        IPaymentResult.Success result = new IPaymentResult.Success(0);
        ShowIAPResult_Success(product, result);
#else
        paymentService?.Request(
            product.Index, 
            onSuccess: result => {
                ShowIAPResult_Success(product, result);
            },
            onError: error => {
                ShowIAPResult_Fail(product, error);
            }
        );
#endif
    }

    public static void ShowIAPResult_Success(ProductData product, IPaymentResult.Success result)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : SUCCESS : {1}", product.ProductId, result.ToString()));

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
            { SkillItemType.Stash,      (int)product.Contents.GetValueOrDefault(1, 0) },
            { SkillItemType.Undo,       (int)product.Contents.GetValueOrDefault(2, 0) },
            { SkillItemType.Shuffle,    (int)product.Contents.GetValueOrDefault(3, 0) }
        };

        long addBooster = product.Contents.GetValueOrDefault(4, 0);

        GetItems(product.Coin, addSkillItems, addBooster);

        SetADFreeUser();
        globalData.ShowPresentPopup(product);
    }

    public static void ShowIAPResult_Fail(ProductData product, IPaymentResult.Error error)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : ERROR : {1}", product.ProductId, error.ToString()));

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