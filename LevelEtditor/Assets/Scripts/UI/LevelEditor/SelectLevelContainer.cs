using UnityEngine;

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

using TMPro;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters.Math;
using Newtonsoft.Json.UnityConverters;

public class SelectLevelContainerParameter
{
	public Action<int> OnTakeStep; 
	public Action<int> OnNavigate; 
	public Action OnSave;
	public IUniTaskAsyncEnumerable<bool> SaveButtonBinder;
	public string DataPath;
	public Action<bool, string> OnVisibleDim;
	public LevelDataManager DataManager;

	public Action<int> OnControlDifficult;

	public Action<Action<string>> OnGetPlayerPath;
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
	private LevelEditorButton m_downDifficultButton;

	[SerializeField]
	private TMP_Text m_difficultText;

	[SerializeField]
	private LevelEditorButton m_upDifficultButton;

	[SerializeField]
	private LevelEditorButton m_saveButton;

	[SerializeField]
	private LevelEditorButton m_playButton;

	[SerializeField]
	private LevelEditorButton m_browserButton;

	private Process m_clientProcess = null;

	public void OnSetup(SelectLevelContainerParameter parameter)
	{
		m_prevButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(-1));
		m_nextButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(1));
		m_downDifficultButton.OnSetup(() => parameter?.OnControlDifficult?.Invoke(-1));
		m_upDifficultButton.OnSetup(() => parameter?.OnControlDifficult?.Invoke(1));
		m_saveButton.OnSetup("Save Level", () => parameter?.OnSave?.Invoke());
		m_playButton.OnSetup(
			() => parameter?.OnGetPlayerPath.Invoke(
				async path => {
					if (string.IsNullOrWhiteSpace(path))
					{
						UnityEngine.Debug.Log("string.IsNullOrWhiteSpace(path)");
						return;
					}		
					await StartProcess(path, this.GetCancellationTokenOnDestroy());
				}
			)
		);

		m_browserButton.OnSetup(() => Application.OpenURL($"file:///{PlayerPrefs.GetString(LevelEditor.DATA_PATH_KEY)}"));

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

		async UniTask StartProcess(string path, CancellationToken token)
		{
			if (m_clientProcess != null)
            {
				m_clientProcess?.Dispose();
				m_clientProcess = null;
            }

			m_clientProcess = new Process();
			m_clientProcess.StartInfo.FileName = path;
			m_clientProcess.Exited += Exited;

			string appDir = Application.persistentDataPath;

			string levelDataDir = Path.Combine(appDir, "LevelDatas");
			UnityEngine.Debug.Log(levelDataDir);

			if (!Directory.Exists(levelDataDir))
			{
				Directory.CreateDirectory(levelDataDir);
			}

			var data = parameter.DataManager.CurrentLevelData;

			string json = JsonConvert.SerializeObject(
				data, 
				new JsonSerializerSettings {
					Converters = new [] {
						new Vector3Converter()
					},
					Formatting = Formatting.Indented,
					ContractResolver = new UnityTypeContractResolver()
				}
			);

			int level = parameter.DataManager.CurrentLevelData.Key;

			string fileName = Path.Combine(levelDataDir, $"LevelData_{level}.json");

			using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					writer.Write(json);
				}
			}

			string temp = Path.Combine(appDir, "mockup");

			if (!Directory.Exists(temp))
			{
				Directory.CreateDirectory(temp);
			}

			string mockupConfig = Path.Combine(temp, "mockup.txt");
			if (File.Exists(mockupConfig))
			{
				File.Delete(mockupConfig);
			}

			using (var fileStream = new FileStream(mockupConfig, FileMode.OpenOrCreate, FileAccess.Write))
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					writer.Write($"{level}");
				}
			}

			parameter.OnVisibleDim.Invoke(true, "게임 실행 중...");
			
			await UniTask.Delay(500,  cancellationToken: token);

			m_clientProcess.Start();

			m_clientProcess.WaitForExit();

			UnityEngine.Debug.LogWarning(m_clientProcess.Id);

			Exited();
		}

		void Exited(object sender = null, System.EventArgs e = null)
		{
			parameter.OnVisibleDim.Invoke(false, string.Empty);
		}

	}

	public void OnUpdateUI(int maxLevel, int level)
	{
		m_prevButton.SetInteractable(level > 1);
		m_nextButton.UpdateUI(level < maxLevel? ">>" : "+");
		m_inputField.SetTextWithoutNotify($"Lv.{level}");
	}

	public void OnUpdateDifficult(DifficultType difficult)
	{
		m_difficultText.text = difficult.ToString();
	}

    private void OnDestroy()
    {
		m_clientProcess?.Dispose();
    }

}
