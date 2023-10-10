using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;

using NineTap.Common;

public class MainSceneTabButton : CachedBehaviour
{
	[SerializeField]
	private Toggle m_toggle;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private MainMenuType m_type;

	public MainMenuType MenuType => m_type;

	private void Reset()
	{
		m_toggle = transform.GetComponent<Toggle>();
		transform.TryGetComponentAtPath("Layout/Text", out m_text);
		transform.TryGetComponentAtPath("Layout/Icon", out m_image);
	}

	public void OnSetup(Action<MainMenuType> onToggle, ToggleGroup toggleGroup, bool awakeOn = false)
	{
		m_image.sprite = MenuType.GetSprite();
		m_text.text = MenuType.GetName();
		m_toggle.group = toggleGroup;
		m_toggle.onValueChanged.AddListener(
			on => {
				if (on)
				{
					onToggle?.Invoke(MenuType);
				}
			}
		);

		SetIsOnWithoutNotify(awakeOn);
	}

	public void SetIsOnWithoutNotify(bool on)
	{
		m_toggle.SetIsOnWithoutNotify(on);
	}
}
