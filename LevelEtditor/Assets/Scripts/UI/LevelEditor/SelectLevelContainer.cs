using UnityEngine;

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

using TMPro;

using Cysharp.Threading.Tasks;

using SimpleFileBrowser;

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

	public void OnSetup(SelectLevelContainerParameter parameter)
	{
		m_prevButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(-1));
		m_nextButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(1));
		m_downDifficultButton.OnSetup(() => parameter?.OnControlDifficult?.Invoke(-1));
		m_upDifficultButton.OnSetup(() => parameter?.OnControlDifficult?.Invoke(1));
		m_saveButton.OnSetup("Save Level", () => parameter?.OnSave?.Invoke());
		m_playButton.OnSetup(
			() => {
				UniTask.Void
				(
					async token => {		
						if (!PlayerPrefs.HasKey("play_app_path"))
						{
							FileBrowser.SetFilters(true, new string[] {"exe", "app"});
							FileBrowser.ShowLoadDialog(
								onSuccess: async paths => {
									string path = paths[0];
									PlayerPrefs.SetString("play_app_path", path);
									await StartProcess(path, token);
								},
								() => Application.Quit(),
								pickMode: FileBrowser.PickMode.Files,
								title: "플레이할 앱 선택",
								loadButtonText: "선택"
							);
							return;
						}

						string appPath = PlayerPrefs.GetString("play_app_path");

						await StartProcess(appPath, token);
					},
					this.GetCancellationTokenOnDestroy()
				);
			}
		);

		m_browserButton.OnSetup(() => Application.OpenURL($"file:///{parameter.DataPath}"));

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
			Process process = new Process();
			process.StartInfo.FileName = path;
			process.Exited += Exited;

			#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
			string appDir = Directory.GetParent(path).FullName;
			#else
			string appDir = Directory.GetCurrentDirectory(); //Directory.GetParent(path).FullName;
			#endif

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

			using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					await writer.WriteAsync(json);
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

			using (var fileStream = new FileStream(mockupConfig, FileMode.OpenOrCreate, FileAccess.ReadWrite))
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					await writer.WriteAsync($"{level}");
				}
			}


			parameter.OnVisibleDim.Invoke(true, "게임 실행 중...");
			
			await UniTask.Delay(500,  cancellationToken: token);

			process.Start();

			process.WaitForExit();

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

}
