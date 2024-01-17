using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GlobalData
{
    public void ShowReadyPopup(Action onComplete = null)
    {
        User user = userManager.Current;

        UIManager.ShowPopupUI<ReadyPopup>(
        new ReadyPopupParameter(
            Level: user.Level,
            ExitParameter: ExitBaseParameter.CancelParam,
            BaseButtonParameter: new UITextButtonParameter{
                ButtonText = NineTap.Constant.Text.Button.PLAY,
                OnClick = () => 
                {
                    var (_, valid, _) = user.Valid();

                    if (!valid)
                    {
                        ShowHeartBuyPopup(() => mainScene.scrollView.MoveTo((int)MainMenuType.STORE));
                        return;
                    }
                    UIManager.ShowSceneUI<PlayScene>(new PlaySceneParameter());
                }
            },
            AllPressToClose: true,
            HUDTypes: HUDType.ALL,
            OnComplete: onComplete
        ));
    }

    public void ShowHeartBuyPopup(Action onClick = null)
    {
        UIManager.ShowPopupUI<GiveupPopup>(
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
        );
    }
}