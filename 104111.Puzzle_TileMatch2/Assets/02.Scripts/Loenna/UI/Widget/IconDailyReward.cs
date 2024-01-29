using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NineTap.Common;

[ResourcePath("UI/Widgets/IconDailyReward")]
public class IconDailyReward : CachedBehaviour
{
    [SerializeField]
	private Image m_image;

	[SerializeField]
	private TMP_Text m_text;

	public void OnSetup(string spriteName, string text = "")
	{
		var sprite = SpriteManager.GetSprite(spriteName);
		m_image.sprite = sprite;
		m_text.text = text;
	}
}
