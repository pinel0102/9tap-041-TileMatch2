using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NineTap.Common;
using TMPro;

[ResourcePath("UI/Widgets/RewardItemEvent")]
public class RewardItemEvent : CachedBehaviour
{
    [SerializeField]	private RectTransform m_rewardItemRect;
    [SerializeField]	private Image m_rewardItemImage;
    [SerializeField]	private TMP_Text m_rewardItemText;
    [SerializeField]	private GameObject m_ribbonObject;
    [SerializeField]	private GameObject m_doubleObject;
    [SerializeField]	private CanvasGroup m_rewardHalo;

	public virtual void Initialize(string spriteName, string countString = null, float spriteSize = 80, bool isRibbon = false, bool isDouble = false)
	{
		m_rewardItemImage.sprite = SpriteManager.GetSprite(spriteName);
        m_rewardItemRect.SetSize(spriteSize);
        m_rewardItemText.SetText(countString);
        m_ribbonObject.SetActive(isRibbon);
        m_doubleObject.SetActive(isDouble);
	}
}
