using UnityEngine;

using NineTap.Payment;
using NineTap.Common;
using Cysharp.Threading.Tasks;

public class ShopProductItem : CachedBehaviour
{
	[SerializeField]
	private ValueWidget m_valueWidget;

	[SerializeField]
	private UITextButton m_button;

	public void UpdateUI(ProductData product)
	{
		m_valueWidget.UpdateUI(product.UIType, product.GetShopItemImagePath(), $"{product.Coin} Golds");

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

        void PurchasedCallback()
        {
            GlobalData.Instance.HUD_Show(GlobalData.Instance.CURRENT_SCENE == GlobalDefine.SCENE_PLAY ? HUDType.COIN : HUDType.ALL);
        }
	}
}
