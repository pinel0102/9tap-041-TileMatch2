using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NineTap.Common;

public static partial class UIManager
{
    private static bool testReviewPopup = false;

    public static void ShowReviewPopup()
    {
        ShowPopupUI<ReviewPopup>(
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
                    OnClick = ClosePopupUI
                },
                CloseButtonParameter: new UITextButtonParameter {
                    ButtonText = NineTap.Constant.Text.Button.OK,
                    OnClick = ClosePopupUI
                }
            )
        );

        void SetRated()
        {            
            m_userManager.Update(isRated: true);

            Debug.Log(CodeManager.GetMethodName() + string.Format("IsRated : {0}", m_userManager.Current.IsRated));
        }
    }

    public static bool IsEnableReviewPopup(int level)
    {
        if(testReviewPopup)
            return true;
        
        // 레이트 여부 체크.
        if(m_userManager.Current.IsRated)
            return false;

        // 하루 2번 체크.
        //return true;

        // 레벨 체크.
        bool canShow = false;

        switch(level)
        {
            case 10:
            case 26:
                canShow = true;
                break;
            case > 30:
                canShow = level % 20 == 0;
                break;
        }

        return canShow;
    }
}
