using UnityEngine;

using NineTap.Common;

public class ShopProductItem : CachedBehaviour
{
	[SerializeField]
	private ValueWidget m_valueWidget;

	[SerializeField]
	private UITextButton m_button;

	public void UpdateUI(ProductData product)
	{
		m_valueWidget.UpdateUI(product.UIType, product.GetShopItemImagePath(), $"{product.Coin} Gold");

		m_button.OnSetup(
			new UITextButtonParameter {
				ButtonText = product.GetPriceString(),
				OnClick = () => Purchase(product)
			}
		);
	}

	private void Purchase(ProductData product)
	{
		
	}
}
