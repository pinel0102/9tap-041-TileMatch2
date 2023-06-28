using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System;

public class SelectLevelContainer : MonoBehaviour
{
	[SerializeField]
	private LevelEditorButton m_prevButton;

	[SerializeField]
	private TMP_InputField m_inputField;

	[SerializeField]
	private LevelEditorButton m_nextButton;

	[SerializeField]
	private LevelEditorButton m_saveButton;

	[SerializeField]
	private LevelEditorButton m_playButton;

	public void OnSetup(Action<int> onMove, Action<int> onJump, Action onSave)
	{
		m_prevButton.OnSetup(() => onMove?.Invoke(-1));
		m_nextButton.OnSetup(() => onMove?.Invoke(1));
		m_saveButton.OnSetup("Save Level", () => onSave?.Invoke());
	}

    public void UpdateUI(int level)
    {
       m_inputField.SetTextWithoutNotify($"Lv.{level}");
    }
}
