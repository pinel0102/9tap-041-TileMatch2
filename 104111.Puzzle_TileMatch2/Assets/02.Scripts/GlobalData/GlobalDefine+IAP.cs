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
        paymentService?.Request(
            product, 
            onSuccess: result => {
                Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : SUCCESS : {1}", product.ProductId, result.ToString()));
            },
            onError: error => {
                Debug.Log(CodeManager.GetMethodName() + string.Format("{0} : ERROR : {1}", product.ProductId, error.ToString()));
            }
        );
    }
}