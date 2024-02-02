using UnityEngine;

using NineTap.Common;

public record BuyItemPopupParameter
(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	UITextButtonParameter BaseButtonParameter,
	ProductData Product
) : PopupBaseParameter(Title, Message, ExitParameter, BaseButtonParameter, true, HUDType.COIN);

[ResourcePath("UI/Popup/BuyItemPopup")]
public class BuyItemPopup : PopupBase
{
	[SerializeField]
	private RectTransform m_productWidgetContainer;

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
		}
	}

    public override void OnShow()
    {
        base.OnShow();

        GlobalData.Instance.SetTouchLock_PlayScene(false);
    }

    public override void OnClickClose()
	{
		UIManager.HUD.Hide();
		base.OnClickClose();
	}
}
