using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Collections.Generic;

using TMPro;

using NineTap.Payment;
using NineTap.Common;
using Cysharp.Threading.Tasks;

public class ShopProductBundleItem : CachedBehaviour
{
	[Serializable]
	private class TagWidget
	{
		[SerializeField]
		private GameObject m_root;

		[SerializeField]
		private TMP_Text m_text;

		public void UpdateUI(bool visible, string text)
		{
			m_root.SetActive(visible);
			m_text.text = text;
		}
	}

	[SerializeField]
	private Image m_background;

	[SerializeField]
	private TMP_Text m_productNameText;

	[SerializeField]
	private Graphic m_infoGraphic;

	[SerializeField]
	private ValueWidget m_goldWidget;

	[SerializeField]
	private List<ValueWidget> m_valueWidgets;

	[SerializeField]
	private Image m_bundleImage;
	
	[SerializeField]
	private UITextButton m_button;

	[SerializeField]
	private TagWidget m_optionalTag;

	[SerializeField]
	private TagWidget m_saleTag;

	public void UpdateUI(ProductData product)
	{
		ItemDataTable itemDataTable = Game.Inst.Get<TableManager>().ItemDataTable;
		m_background.sprite = SpriteManager.GetSprite(product.UIType.GetBundleImagePath());
		m_infoGraphic.color = product.UIType.GetBundleInfoColor();

		m_productNameText.text = product.FullName;
		m_bundleImage.sprite = SpriteManager.GetSprite(product.GetShopItemImagePath());

		m_optionalTag.UpdateUI(!string.IsNullOrWhiteSpace(product.Description), product.Description);
		m_saleTag.UpdateUI(product.DiscountPercent > 0, product.GetSaleString());

		m_goldWidget.UpdateUI(product.UIType, "UI_Shop_Icon_Coins", $"{product.Coin} Golds");

		m_button.onClick.RemoveAllListeners();
		m_button.OnSetup(
			new UITextButtonParameter {
				ButtonText = string.Empty,
                ButtonTextBinder = new AsyncReactiveProperty<string>(product.GetPriceString()),
				OnClick = () => {
                    GlobalDefine.Purchase(product, PurchasedCallback, PurchasedCallback);
                }
			}
		);

		IReadOnlyDictionary<int, long> contents = product.Contents;

		for (int i = 0, count = m_valueWidgets.Count; i < count; i++)
		{
			ValueWidget valueWidget = m_valueWidgets[i];
			if (i >= contents.Count)
			{
				valueWidget.SetVisible(false);
				continue;
			}

			(int index, long value) = contents.ElementAt(i);

			if (itemDataTable.TryGetValue(index, out ItemData itemData))
			{
				valueWidget.SetVisible(true);
				valueWidget.UpdateUI(product.UIType, itemData.ImagePath, itemData.ToString(value));
				continue;
			}
			
			valueWidget.SetVisible(false);
		}

        void PurchasedCallback()
        {
            GlobalData.Instance.HUD_Show(GlobalData.Instance.CURRENT_SCENE == GlobalDefine.SCENE_PLAY ? HUDType.COIN : HUDType.ALL);
        }
	}
}
