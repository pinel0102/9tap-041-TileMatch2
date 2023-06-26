using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class SelectLevelContainer : MonoBehaviour
{
	[SerializeField]
	private Button m_prevButton;

	[SerializeField]
	private TMP_InputField m_inputField;

	[SerializeField]
	private Button m_nextButton;

	[SerializeField]
	private Button m_loadButton;

	[SerializeField]
	private Button m_saveButton;

	[SerializeField]
	private Button m_playButton;

	public void OnSetup()
	{

	}
}
