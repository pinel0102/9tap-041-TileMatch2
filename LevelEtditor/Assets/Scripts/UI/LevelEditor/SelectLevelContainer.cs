using UnityEngine;
using UnityEngine.UI;

using System;
using System.Diagnostics;

using TMPro;

using Cysharp.Threading.Tasks;
using System.Collections;
using System.IO;
using System.IO.MemoryMappedFiles;
using SimpleFileBrowser;
using System.Threading;
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

	private int m_currentLevel = 0;

	public void OnSetup(SelectLevelContainerParameter parameter)
	{
		m_prevButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(-1));
		m_nextButton.OnSetup(() => parameter?.OnTakeStep?.Invoke(1));
		m_saveButton.OnSetup("Save Level", () => parameter?.OnSave?.Invoke());
		m_playButton.OnSetup(
			() => {
				UniTask.Void
				(
					async token => {		
						//string path = Environment.GetEnvironmentVariable("tile_match_2_client", EnvironmentVariableTarget.User);

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

						//using (var mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.OpenOrCreate, $"LevelData"))
						//{
						//	parameter.OnVisibleDim.Invoke(true, "게임 실행 중...");
							
						//	await UniTask.Delay(500,  cancellationToken: token);

						//	process.Start();
						//	UnityEngine.Debug.Log(process.Id);

						//	process.WaitForExit();

						//	Exited();
						//}
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

			string appDir = Directory.GetParent(path).Parent.Parent.Parent.FullName;
			UnityEngine.Debug.LogWarning(appDir);

			UnityEngine.Debug.Log(Directory.GetParent(path));

			string levelDataDir = Path.Combine(appDir, "LevelDatas");

			if (!Directory.Exists(levelDataDir))
			{
				Directory.CreateDirectory(levelDataDir);
			}

			foreach (var (key, value) in parameter.DataManager.CachedLevelDataDic)
			{
				string json = JsonConvert.SerializeObject(
					value, 
					new JsonSerializerSettings {
						Converters = new [] {
							new Vector3Converter()
						},
						Formatting = Formatting.Indented,
						ContractResolver = new UnityTypeContractResolver()
					}
				);

				string fileName = Path.Combine(levelDataDir, $"LevelData_{key}.json");

				using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					using (StreamWriter writer = new StreamWriter(fileStream))
					{
						await writer.WriteAsync(json);
					}
				}
			}

			string temp = Path.Combine(appDir, "temp");

			if (!Directory.Exists(temp))
			{
				Directory.CreateDirectory(temp);
			}

			string mockupConfig = Path.Combine(temp, "Mockup.text");

			using (var fileStream = new FileStream(mockupConfig, FileMode.OpenOrCreate, FileAccess.ReadWrite))
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					await writer.WriteAsync($"{m_currentLevel}");
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
		m_currentLevel = level;
		m_prevButton.SetInteractable(level > 1);
		m_nextButton.UpdateUI(level < maxLevel? ">>" : "+");
		m_inputField.SetTextWithoutNotify($"Lv.{level}");
	}

}
