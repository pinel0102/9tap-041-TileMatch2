using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;

using NineTap.Common;

public class EventOptionalContainer : CachedBehaviour
{
    [Serializable]
	public class Widget
	{
		[SerializeField]
		protected GameObject m_root;

		public bool SetActive(bool active)
		{
			m_root.SetActive(active);
			return active;
		}
	}

	[Serializable]
	public class TextWidget : Widget
	{
		[SerializeField]
		protected TMP_Text m_text;

		public void SetText(string text)
		{
			m_text.text = text;
		}
	}

	[Serializable]
	public class TextWithImageWidget : TextWidget
	{
		[SerializeField]
		protected Image m_image;

		public void SetSprite(Sprite sprite)
		{
			m_image.sprite = sprite;
		}
	}

	[SerializeField]
	public TextWidget m_speechBubbleWidget;

	[SerializeField]
	public TextWithImageWidget m_labelWidget;

	[SerializeField]
	public TextWithImageWidget m_timeWidget;

	[SerializeField]
	public TextWidget m_discountWidget;

	[SerializeField]
	public Widget m_adWidget;

	public void OnUpdateUI(ProductData product)
	{
		if (m_speechBubbleWidget.SetActive(product.UIType is UIType.Piggy))
		{
			// 모은 골드 개수
		}

		if (m_labelWidget.SetActive(
				product.UIType is not UIType.Piggy
				&& product.Required is ProductExposingType.Home or ProductExposingType.StoreAndHome
			)
		)
		{
			m_labelWidget.SetText(product.SimplifiedName);
			m_labelWidget.SetSprite(SpriteManager.GetSprite(product.UIType.GetEventLabelImagePath()));
		}

		if (m_timeWidget.SetActive(
				product.UIType is not UIType.Piggy
				&& product.Required is ProductExposingType.Home or ProductExposingType.StoreAndHome
			)
		)
		{
			// 노출 시간
			//m_timeWidget.SetText(DateTime.Now.ToShortTimeString());
            m_timeWidget.SetText(string.Empty);
			m_timeWidget.SetSprite(SpriteManager.GetSprite(product.UIType.GetEventTimeImagePath()));
		}

		if (m_discountWidget.SetActive(product.DiscountPercent > 0))
		{
			m_discountWidget.SetText(product.GetSaleString());
		}

		m_adWidget.SetActive(false); //광고
	}
}
