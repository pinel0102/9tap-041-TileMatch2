using UnityEngine;
using UnityEngine.UI;

using System;
using System.Diagnostics;

using TMPro;

using Cysharp.Threading.Tasks;
using System.Collections;
using System.IO;

public class SelectLevelContainerParameter
{
	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate; 
	public Action OnSave;
	public IUniTaskAsyncEnumerable<bool> SaveButtonBinder;
	public string FolderPath;
	public Action<bool, string> OnVisibleDim;
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
				string path = Environment.GetEnvironmentVariable(
						"tile_match_2_client", 
						EnvironmentVariableTarget.User
					);
				// string path = Registry.CurrentUser
				// 	.CreateSubKey("Software")
				// 	.OpenSubKey("DefaultCompany")
				// 	.OpenSubKey("Client")
				// 	.GetValue("application_path")
				// 	.ToString();
				
				UnityEngine.Debug.Log(path);

				parameter.OnVisibleDim.Invoke(true, "게임 실행 중...");
				
				//await UniTask.Delay(500,  cancellationToken: token);

				using (var process = Process.Start(path))
				{
					process.WaitForExit();
				}

				parameter.OnVisibleDim.Invoke(false, string.Empty);
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
