using UnityEngine;
using UnityEngine.UI;

using System;

using NineTap.Common;

public class EventCircleIconParameter
{
	public ProductData Product;
}

[ResourcePath("UI/Widgets/EventCircleIcon")]
public class EventCircleIcon : CachedBehaviour
{
	[Serializable]
	private class IconWidget
	{
		[SerializeField]
		private Image m_circle;

		[SerializeField]
		private Image m_icon;

		public void UpdateUI(Sprite circle, Sprite icon)
		{
			m_circle.sprite = circle;

			m_icon.color = icon == null? Color.clear : Color.white;
			m_icon.sprite = icon;
		}

		public void UpdateUI(float fillAmount)
		{
			m_circle.fillAmount = fillAmount;
		}
	}

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private IconWidget m_grayScaleIcon;

	[SerializeField]
	private IconWidget m_mainIcon;

	[SerializeField]
	private EventOptionalContainer m_optionalContainer;

	[SerializeField]
	private UIImageButton m_button;

	public void OnSetup(EventCircleIconParameter parameter)
	{
		
	}

	public void OnUpdateUI(ProductData productData)
	{
		Sprite circleSprite = SpriteManager.GetSprite(productData.UIType.GetCircleIconPath());
		Sprite iconSprite = SpriteManager.GetSprite(productData.GetLobbyItemImagePath());
		m_grayScaleIcon.UpdateUI(circleSprite, iconSprite);
		m_mainIcon.UpdateUI(circleSprite, iconSprite);
		m_optionalContainer.OnUpdateUI(productData);
	}
}
