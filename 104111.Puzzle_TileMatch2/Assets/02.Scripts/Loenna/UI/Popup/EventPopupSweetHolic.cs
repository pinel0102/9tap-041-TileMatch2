using System.Collections;
using System.Collections.Generic;
using NineTap.Common;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using DG.Tweening;

public record EventPopupSweetHolicParameter
(
	string Title,
	Action PopupCloseCallback,
    params HUDType[] VisibleHUD
) : UIParameter(VisibleHUD);


[ResourcePath("UI/Popup/EventPopupSweetHolic")]
public class EventPopupSweetHolic : UIPopup
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private UIImageButton closeButton;
    private Action m_popupCloseCallback;

    public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not EventPopupSweetHolicParameter parameter)
		{
			return;
		}

        m_popupCloseCallback = parameter.PopupCloseCallback;

        titleText.SetText(parameter.Title);        
        closeButton.OnSetup(new UIImageButtonParameter{
            OnClick = OnClickClose
        });
    }

    public override void OnShow()
    {
        base.OnShow();

        GlobalData.Instance.HUD_Hide();
    }

    public override void OnHide()
    {
        base.OnHide();

        m_popupCloseCallback?.Invoke();

        if (GlobalData.Instance.CURRENT_SCENE == GlobalDefine.SCENE_PLAY)
            GlobalData.Instance.HUD_Hide();
        else
            GlobalData.Instance.HUD_Show(HUDType.ALL);
    }

    public override void OnClickClose()
	{
		base.OnClickClose();
	}
}
