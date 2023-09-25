using UnityEngine;
using UnityEngine.SceneManagement;

using System;

using TMPro;

using Cysharp.Threading.Tasks;

using SceneManagement = UnityEngine.SceneManagement;

public class SelectLevelContainerParameter
{
	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate; 
	public Action OnSave;
	public Action OnPlay;
	public Action<bool> OnChangeMode;
	public IUniTaskAsyncEnumerable<bool> SaveButtonBinder;
	public Action<int> OnControlDifficult;
}

public class SelectLevelContainer : MonoBehaviour
{
	public const string DATA_PATH_KEY = "data_path";

	[SerializeField]
	private LevelEditorButton m_prevButton;

	[SerializeField]
	private TMP_InputField m_inputField;

	[SerializeField]
	private LevelEditorButton m_nextButton;

	[SerializeField]
	private LevelEditorButton m_downDifficultButton;

	[SerializeField]
	private TMP_Text m_difficultText;

	[SerializeField]
	private LevelEditorButton m_generateButton;

	[SerializeField]
	private LevelEditorButton m_upDifficultButton;

	[SerializeField]
	private LevelEditorButton m_saveButton;

	[SerializeField]
	private LevelEditorButton m_playButton;

	[SerializeField]
	private LevelEditorToggleButton m_modeButton;

	public void OnSetup(SelectLevelContainerParameter parameter)
	{
		SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
		m_prevButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(-1));
		m_nextButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(1));
		m_downDifficultButton.OnSetup(() => parameter?.OnControlDifficult?.Invoke(-1));
		m_upDifficultButton.OnSetup(() => parameter?.OnControlDifficult?.Invoke(1));
		m_saveButton.OnSetup("Save Level", () => parameter?.OnSave?.Invoke());
		m_playButton.OnSetup(() => parameter?.OnPlay?.Invoke());

		parameter?.SaveButtonBinder?.BindTo(m_saveButton, (button, interactable) => button.SetInteractable(interactable));
		parameter?.SaveButtonBinder?.BindTo(m_playButton, (button, interactable) => button.SetInteractable(interactable));

		m_inputField.onEndEdit.AddListener(
			text => {
				parameter?.OnNavigate?.Invoke(
					int.TryParse(text, out int result) switch {
						true => result,
						_=> -1
					}
				);
			}
		);

		m_modeButton.OnSetup(string.Empty, parameter.OnChangeMode, false);
	}

	private void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		
	}

	public void OnUpdateUI(int maxLevel, int level)
	{
		m_prevButton.SetInteractable(level > 1);
		m_nextButton.UpdateUI(level < maxLevel? ">>" : "+");
		m_inputField.SetTextWithoutNotify($"Lv.{level}");
	}

	public void OnUpdateGrades(DifficultType difficult, bool hardMode)
	{
		m_difficultText.text = difficult.ToString();
		m_modeButton.SetIsOnWithoutNotify(hardMode);
	}
}
