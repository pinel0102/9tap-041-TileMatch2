using UnityEngine;
using System;
using NineTap.Common;
using TMPro;

public record ProductPopupParameter
(
	string Title,
	string Message,
	ExitBaseParameter ExitParameter,
	ProductData ProductData,
    string BackgroundImage,
    string RibbonImage,
    string CoinImage,
    Action PurchasedCallback,
    Action PopupCloseCallback,
    params HUDType[] HUDTypes
) : PopupBaseParameter(Title, Message, ExitParameter, null, false, HUDTypes);

[ResourcePath("UI/Popup/ProductPopup")]
public class ProductPopup : PopupBase
{
    [SerializeField] private GameObject m_titleObjectRibbon;
    [SerializeField] private DynamicImage m_ribbonImage;
    [SerializeField] private DynamicImage m_backgroundImage;
    [SerializeField] private GameObject m_titleObjectNone;
    [SerializeField] private TMP_Text m_titleTextNone;
    [SerializeField] private DynamicImage m_coinImage;
    [SerializeField] private TMP_Text m_coinText;
    [SerializeField] private GameObject m_saleObject;
	[SerializeField] private TMP_Text m_saleText;
    [SerializeField] private RectTransform m_productWidgetContainer;

    private ProductData productData;
    private Action m_purchasedCallback;
    private Action m_popupCloseCallback;
    private bool useRibbon;

	public override void OnSetup(UIParameter uiParameter)
	{
		base.OnSetup(uiParameter);

        if (uiParameter is ProductPopupParameter parameter)
		{
			if (parameter.ProductData?.Contents?.Count <= 0)
			{
				return;
			}

            productData = parameter.ProductData;
            m_purchasedCallback = parameter.PurchasedCallback;
            m_popupCloseCallback = parameter.PopupCloseCallback;

            m_titleTextNone.SetText(parameter.Title);
            useRibbon = !string.IsNullOrEmpty(parameter.RibbonImage);
            if (useRibbon)
                m_ribbonImage.ChangeSprite(parameter.RibbonImage);
            m_backgroundImage.ChangeSprite(parameter.BackgroundImage);
            m_coinImage.ChangeSprite(parameter.CoinImage);
            m_titleObjectRibbon.SetActive(useRibbon);
            m_titleObjectNone.SetActive(!useRibbon);
            m_coinText.SetText(productData.Coin.ToString());
            m_saleText.SetText(productData.GetSaleString());
            m_saleObject.SetActive(productData.DiscountPercent > 0);

			ItemDataTable itemDataTable = Game.Inst.Get<TableManager>().ItemDataTable;

			var prefab = ResourcePathAttribute.GetResource<SimpleProductItemWidgetSmall>();
			foreach (var (index, count) in productData.Contents)
			{
				if (itemDataTable.TryGetValue(index, out var itemData))
				{
					var widget = Instantiate(prefab);
					widget.CachedTransform.SetParentReset(m_productWidgetContainer);
					widget.OnUpdateUI(itemData.ImagePath, itemData.ToString(count));
				}
			}

            m_button.OnSetup(new UITextButtonParameter {
                OnClick = () => {
                    GlobalDefine.Purchase(productData, onSuccess: OnPurchased);
                },
                ButtonText = productData.GetPriceString(),
                SubWidgetBuilder = null
            });
		}
	}

    public override void OnShow()
    {
        base.OnShow();
    }

    public override void OnHide()
    {
        base.OnHide();

        if (GlobalData.Instance.CURRENT_SCENE == GlobalDefine.SCENE_PLAY)
            GlobalData.Instance.HUD.Hide();
        else
            GlobalData.Instance.HUD.Show(HUDType.ALL);

        m_popupCloseCallback?.Invoke();
    }

    public void OnPurchased()
    {
        m_purchasedCallback?.Invoke();

        OnClickClose();
    }
}
