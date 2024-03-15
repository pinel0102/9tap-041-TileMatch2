using UnityEngine;
using UnityEngine.UI;

using TMPro;

using NineTap.Common;

[ResourcePath("UI/Widgets/IconWidget")]
public class IconWidget : CachedBehaviour
{
	[SerializeField]
	private Image m_image;

	[SerializeField]
	private TMP_Text m_text;

    [SerializeField]
	private LayoutElement m_layout;

	public void OnSetup(string spriteName, string text = "", bool isGray = false)
	{
		var sprite = SpriteManager.GetSprite(spriteName);
		m_image.sprite = sprite;
		m_text.text = text;
		m_image.color = isGray? Color.gray : Color.white;
		m_text.color = isGray? Color.gray : Color.white;
	}

    public void OnSetup(string spriteName, float iconSize, string text = "", bool isGray = false)
	{
		var sprite = SpriteManager.GetSprite(spriteName);
		m_image.sprite = sprite;
		m_text.text = text;
		m_image.color = isGray? Color.gray : Color.white;
		m_text.color = isGray? Color.gray : Color.white;

        m_layout.preferredWidth = iconSize;
        m_layout.preferredHeight = iconSize;
        m_layout.minWidth = iconSize;
        m_layout.minHeight = iconSize;
	}

    public void SetInteractable(bool interactable)
    {
        m_image.color = interactable? Color.white : Color.gray;
		m_text.color = interactable? Color.white : Color.gray;
    }
}
