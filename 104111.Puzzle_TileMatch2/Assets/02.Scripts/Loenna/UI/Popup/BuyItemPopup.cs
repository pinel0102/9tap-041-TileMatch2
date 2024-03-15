using UnityEngine;
using NineTap.Common;
using System;

public record BuyItemPopupParameter
(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
	ProductData Product,
    Action OnOpened,
    Action<bool> OnClosed,
    bool IgnoreBackKey
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, true, HUDType.COIN);

[ResourcePath("UI/Popup/BuyItemPopup")]
public class BuyItemPopup : PopupBase
{
	[SerializeField]
	private RectTransform m_productWidgetContainer;
    private Action m_onOpened;
    private Action<bool> m_onClosed;
    private bool m_openShop;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

		if (uiParameter is BuyItemPopupParameter parameter)
		{
			if (parameter.Product?.Contents?.Count <= 0)
			{
				return;
			}

			ItemDataTable itemDataTable = Game.Inst.Get<TableManager>().ItemDataTable;
            ignoreBackKey = parameter.IgnoreBackKey;
            m_onOpened = parameter.OnOpened;
            m_onClosed = parameter.OnClosed;
            m_openShop = false;

            SimpleProductItemWidget prefab = ResourcePathAttribute.GetResource<SimpleProductItemWidget>();
			foreach (var (index, count) in parameter.Product.Contents)
			{
				if (itemDataTable.TryGetValue(index, out var itemData))
				{
					SimpleProductItemWidget widget = Instantiate(prefab);
					widget.CachedTransform.SetParentReset(m_productWidgetContainer);
					widget.OnUpdateUI(itemData.ImagePath, itemData.ToString(count));
				}
			}

            m_button.onClick.RemoveAllListeners();
            m_button.onClick.AddListener(() => {
                
                m_openShop = true;
                
                OnClickClose();
                parameter.BaseButtonParameter.OnClick.Invoke();
            });
		}
	}

    public override void OnShow()
    {
        base.OnShow();
        GlobalData.Instance.SetTouchLock_PlayScene(false);
        m_onOpened?.Invoke();
    }

    public override void OnHide()
	{
        base.OnHide();
        GlobalData.Instance.HUD_Preferred();
        m_onClosed?.Invoke(m_openShop);
	}
}
