using UnityEngine;
using UnityEngine.UI;

using System;
using TMPro;

public class LevelEditorButton : MonoBehaviour
{
	[SerializeField]
	private Button m_button;

	[SerializeField]
	private TMP_Text m_textField;

	public void OnSetup(string text, Action onClick)
	{
		m_textField.text = text;
		m_button.onClick.AddListener(() => onClick?.Invoke());
	}
}
