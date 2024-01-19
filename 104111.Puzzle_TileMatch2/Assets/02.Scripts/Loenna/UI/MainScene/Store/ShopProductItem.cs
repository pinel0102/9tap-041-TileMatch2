using UnityEngine;

using NineTap.Payment;
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

        IPaymentService paymentService = Game.Inst.Get<IPaymentService>();

        m_button.onClick.RemoveAllListeners();
		m_button.OnSetup(
			new UITextButtonParameter {
				ButtonText = product.GetPriceString(),
				OnClick = () => {
                    GlobalDefine.Purchase(product);
                }
			}
		);
	}
}
