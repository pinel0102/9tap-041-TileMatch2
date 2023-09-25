using UnityEngine;
using UnityEngine.UI;

using TMPro;

using NineTap.Common;

public class ValueWidget : CachedBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Graphic m_background;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private TMP_Text m_text;

	public void SetVisible(bool visible)
	{
		m_canvasGroup.alpha = visible? 1f : 0f;
	}

	public void UpdateUI(UIType type, string imagePath, string value)
	{
		m_background.color = type.GetValueWidgetColor();
		m_image.sprite = SpriteManager.GetSprite(imagePath);
		m_text.text = value;
	}
}
