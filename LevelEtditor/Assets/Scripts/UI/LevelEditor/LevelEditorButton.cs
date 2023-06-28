using UnityEngine;
using UnityEngine.UI;

using System;
using TMPro;

public class LevelEditorButton : MonoBehaviour
{
	[SerializeField]
	private Button m_button = null!;

	[SerializeField]
	private TMP_Text m_textField = null!;

	public void OnSetup(Action onClick)
	{
		m_button.onClick.AddListener(() => onClick?.Invoke());
	}

	public void OnSetup(string text, Action onClick)
	{
		m_textField.text = text;
		m_button.onClick.AddListener(() => onClick?.Invoke());
	}
}
