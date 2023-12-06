using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NineTap.Common;

public record FreeCoinPopupParameter
(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
    ItemData ItemData
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, false, HUDType.ALL);

[ResourcePath("UI/Popup/FreeCoinPopup")]
public class FreeCoinPopup : PopupBase
{
	[SerializeField]
	private RectTransform m_productWidgetContainer;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is FreeCoinPopupParameter parameter)
		{
			SimpleProductItemWidget prefab = ResourcePathAttribute.GetResource<SimpleProductItemWidget>();

            var itemData = parameter.ItemData;
            int count = 1;

			SimpleProductItemWidget widget = Instantiate(prefab);
            widget.CachedTransform.SetParentReset(m_productWidgetContainer);
            widget.OnUpdateUI(itemData.ImagePath, itemData.ToString(count));
		}
	}

	public override void OnClickClose()
	{
        UIManager.HUD.Show(HUDType.ALL);
		base.OnClickClose();
	}
}