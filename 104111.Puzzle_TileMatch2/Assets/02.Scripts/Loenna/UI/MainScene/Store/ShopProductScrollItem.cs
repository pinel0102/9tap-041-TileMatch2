using UnityEngine;

using Gpm.Ui;

public class ShopProductScrollItemData : InfiniteScrollData
{
	public ProductData ProductData;
}

public class ShopProductScrollItem : NestedInfiniteScrollItem
{
	[SerializeField]
	private ShopProductBundleItem m_productBundle;

	[SerializeField]
	private ShopProductItem m_product;

	public override void UpdateData(InfiniteScrollData scrollData)
	{
		if (scrollData is ShopProductScrollItemData itemData)
		{
			ProductData productData = itemData.ProductData;
			switch (productData.UIType)
			{
				case UIType.Common:
				case UIType.Uncommon:
				case UIType.Epic:
					m_productBundle.CachedGameObject.SetActive(true);
					m_product.CachedGameObject.SetActive(false);
					m_productBundle.UpdateUI(productData);
					break;
				default:
					m_productBundle.CachedGameObject.SetActive(false);
					m_product.CachedGameObject.SetActive(true);
					m_product.UpdateUI(productData);
					break;
			}
		}
	}
}
