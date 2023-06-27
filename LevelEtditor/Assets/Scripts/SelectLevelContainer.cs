using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class SelectLevelContainer : MonoBehaviour
{
	[SerializeField]
	private LevelEditorButton m_prevButton;

	[SerializeField]
	private TMP_InputField m_inputField;

	[SerializeField]
	private LevelEditorButton m_nextButton;

	[SerializeField]
	private LevelEditorButton m_createButton;

	[SerializeField]
	private LevelEditorButton m_saveButton;

	[SerializeField]
	private LevelEditorButton m_playButton;

	public void OnSetup()
	{

	}
}
