using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;

using NineTap.Common;

public class UIToggleButtonParameter
{
	public string Text;
	public Action<bool> OnToggle; 
	public bool AwakeOn = true;
	public ToggleGroup ToggleGroup = null;
	public Func<bool, string> IconBuilder = null;

	public void Deconstruct
	(
		out string text, 
		out Action<bool> onToggle, 
		out bool awakeOn, 
		out ToggleGroup toggleGroup, 
		out Func<bool, string> builder
	)
	{
		text = Text;
		onToggle = OnToggle;
		awakeOn = AwakeOn;
		toggleGroup = ToggleGroup;
		builder = IconBuilder;
	}
}

public class UIToggleButton : CachedBehaviour
{
	[SerializeField]
	private Toggle m_toggle;

	[SerializeField]
	private TMP_Text m_textField;

	[SerializeField]
	private Image m_icon;

	public void OnSetup(UIToggleButtonParameter parameter)
	{
		var (text, onToggle, awakeOn, group, builder) = parameter;

		m_toggle.group = group;
		m_textField.text = text;

        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
		m_toggle.onValueChanged.AddListener(isOn => {
                onToggle?.Invoke(isOn);
                soundManager?.PlayFx(Constant.Sound.SFX_BUTTON);
				SetToggleIcon(builder, isOn);
			}
		);

		SetToggleIcon(builder, awakeOn);
		m_toggle.SetIsOnWithoutNotify(awakeOn);

		void SetToggleIcon(Func<bool, string> builder, bool value)
		{
			if (builder != null)
			{
				m_icon.sprite = SpriteManager.GetSprite(builder?.Invoke(value));
			}
		}
	}

	public void SetIsOnWithoutNotify(bool toggle)
	{
		m_toggle.SetIsOnWithoutNotify(toggle);
	}
}
