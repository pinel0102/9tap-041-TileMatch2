using UnityEngine;
using NineTap.Common;
using System;

public record RemoveAdsPopupParameter
(
	string Title,
	string Message,
	Action PopupCloseCallback,
	ProductData Product,
    params HUDType[] VisibleHUD
) : UIParameter(VisibleHUD);

[ResourcePath("UI/Popup/RemoveAdsPopup")]
public class RemoveAdsPopup : UIPopup
{
    [SerializeField] private UIImageButton closeButton;
    [SerializeField] private UITextButton confirmButton;
	private Action m_popupCloseCallback;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is not RemoveAdsPopupParameter parameter)
		{
			return;
		}

        m_popupCloseCallback = parameter.PopupCloseCallback;

        closeButton.OnSetup(new UIImageButtonParameter{
            OnClick = OnClickClose
        });

        confirmButton.OnSetup(new UITextButtonParameter {
            OnClick = () => {
                GlobalDefine.Purchase(parameter.Product, onSuccess: OnClickClose);
            },
            ButtonText = parameter.Product.GetPriceString(),
            SubWidgetBuilder = null
        });
	}

    public override void OnShow()
    {
        base.OnShow();
    }

    public override void OnClickClose()
	{
		base.OnClickClose();

        m_popupCloseCallback?.Invoke();
	}
}
