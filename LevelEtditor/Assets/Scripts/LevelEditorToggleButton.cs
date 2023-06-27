using UnityEngine;
using UnityEngine.UI;

using System;
using TMPro;

public class LevelEditorToggleButton : MonoBehaviour
{
	[SerializeField]
	private Toggle m_toggle;

	[SerializeField]
	private TMP_Text m_textField;

	public void OnSetup(string text, Action<bool> onToggle, bool awakeOn = true, ToggleGroup group = null)
	{
		m_toggle.group = group;
		m_textField.text = text;

		m_toggle.onValueChanged.AddListener(isOn => onToggle?.Invoke(isOn));
		m_toggle.isOn = awakeOn;
	}

}
