using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NineTap.Payment;

public static partial class GlobalDefine
{
    public static void Purchase(ProductData product)
    {
        IPaymentService paymentService = Game.Inst.Get<IPaymentService>();

        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : {1}", product.ProductId, product.FullName));

#if UNITY_EDITOR
        IPaymentResult.Success result = new IPaymentResult.Success(0);
        ShowIAPResult_Success(product, result);
#else
        paymentService?.Request(
            product, 
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

        // [TODO] IAP Result

        //product.Coin
        //product.Contents
        // [1] : Return
        // [2] : Undo
        // [3] : Shuffle
        // [4] : Infinity Heart (Min)

        /*UIManager.ShowPopupUI<RewardPopup>(
            new RewardPopupParameter (
                PopupType: RewardPopupType.CHEST,
                Reward: rewardData,
                HUDType.COIN
            )
        );*/
    }

    public static void ShowIAPResult_Fail(ProductData product, IPaymentResult.Error error)
    {
        Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : ERROR : {1}", product.ProductId, error.ToString()));

        //
    }
}