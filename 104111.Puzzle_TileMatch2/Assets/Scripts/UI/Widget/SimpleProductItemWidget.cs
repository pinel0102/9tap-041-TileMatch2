using UnityEngine;
using UnityEngine.UI;

using TMPro;

using NineTap.Common;

[ResourcePath("UI/Widgets/SimpleProductItemWidget")]
public class SimpleProductItemWidget : CachedBehaviour
{
    [SerializeField]
	private Image m_icon;

	[SerializeField]
	private TMP_Text m_text;

	public void OnUpdateUI(string iconName, string count)
	{
		m_icon.sprite = SpriteManager.GetSprite(iconName);
		m_text.text = count;
	}
}
