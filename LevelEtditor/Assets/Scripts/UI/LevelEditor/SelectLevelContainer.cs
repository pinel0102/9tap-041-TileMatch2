using UnityEngine;
using UnityEngine.UI;

using System;

using TMPro;

using Cysharp.Threading.Tasks;
using SimpleFileBrowser;

public class SelectLevelContainerParameter
{
	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate; 
	public Action OnSave;
	public IUniTaskAsyncEnumerable<bool> SaveButtonBinder;
	public string FolderPath;
}

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

	[SerializeField]
	private LevelEditorButton m_browserButton;

	public void OnSetup(SelectLevelContainerParameter parameter)
	{
		m_prevButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(-1));
		m_nextButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(1));
		m_saveButton.OnSetup("Save Level", () => parameter?.OnSave?.Invoke());
		m_playButton.OnSetup(
			() => {
#if UNITY_EDITOR
				string applicationType = NativeFilePicker.ConvertExtensionToFileType("exe");

				NativeFilePicker.PickFile(
					path => {
						Application.OpenURL($"file:///{path}");
					},
					new string[] { applicationType }
				);
#else
				FileBrowser.ShowLoadDialog(
					onSuccess: paths => {
						string path = paths[0];
						Application.OpenURL($"file:///{path}");
					},
					() => {},
					pickMode: FileBrowser.PickMode.Folders,
					title: "실행할 어플리케이션 선택",
					loadButtonText: "선택"
				);
#endif
			}
		);
		m_browserButton.OnSetup(() => Application.OpenURL($"file:///{parameter.FolderPath}"));

		parameter?.SaveButtonBinder?.BindTo(m_saveButton, (button, interactable) => button.SetInteractable(interactable));

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
	}

	public void OnUpdateUI(int maxLevel, int level)
	{
		m_prevButton.SetInteractable(level > 1);
		m_nextButton.UpdateUI(level < maxLevel? ">>" : "+");
		m_inputField.SetTextWithoutNotify($"Lv.{level}");
	}

}
