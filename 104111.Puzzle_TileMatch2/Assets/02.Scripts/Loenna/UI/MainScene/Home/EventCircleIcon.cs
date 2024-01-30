using UnityEngine;
using UnityEngine.UI;

using System;

using NineTap.Common;

public class EventCircleIconParameter
{
	public ProductData Product;
    public Action OnClick;
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
        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
		m_button.onClick.AddListener(() => {    
                soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
                parameter.OnClick?.Invoke(); 
            });
	}

	public void OnUpdateUI(ProductData productData)
	{
		Sprite circleSprite = SpriteManager.GetSprite(productData.UIType.GetCircleIconPath());
		Sprite iconSprite = SpriteManager.GetSprite(productData.GetLobbyItemImagePath());
		m_grayScaleIcon.UpdateUI(circleSprite, iconSprite);
		m_mainIcon.UpdateUI(circleSprite, iconSprite);
		m_optionalContainer.OnUpdateUI(productData);
	}

    public void RefreshTime(bool _active, string _text)
    {
        m_optionalContainer.m_timeWidget.SetActive(_active);
        m_optionalContainer.m_timeWidget.SetText(_text);
    }
}
