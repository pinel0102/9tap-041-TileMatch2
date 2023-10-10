using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;

using NineTap.Common;

public class BoosterToggleButtonParameter
{
	public string SpriteName;
	public int Count;
	public Action<int> onToggle;
}

public class BoosterToggleButton : CachedBehaviour
{
	[SerializeField]
	private Image m_image;

	[SerializeField]
	private Toggle m_toggle;

	[SerializeField]
	private TMP_Text m_countText;

	public void OnSetup(BoosterToggleButtonParameter parameter)
	{
		int currentCount = parameter.Count;
		m_toggle.onValueChanged.AddListener(
			isOn => {
				if (currentCount <= 0)
				{
					m_toggle.SetIsOnWithoutNotify(false);
					// 구매 팝업으로
					return;
				}

				int count = isOn? currentCount - 1 : currentCount;
				parameter?.onToggle(count);
				UpdateUI(count);
			}
		);

		UpdateUI(currentCount);
	}

	private void UpdateUI(int count)
	{
		m_countText.text = count switch {
			> 0 => count.ToString(),
			_ => "+"
		};
	}
}
