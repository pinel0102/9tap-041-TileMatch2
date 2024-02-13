using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

public class TableManager
{
	private LevelDataTable m_levelDataTable = new();
	public LevelDataTable LevelDataTable => m_levelDataTable;

	private CountryCodeDataTable m_countryCodeDataTable = new("DB/GameDataTable/CountryCodeDataTable");
	public CountryCodeDataTable CountryCodeDataTable => m_countryCodeDataTable;

	private TileDataTable m_tileDataTable = new("DB/GameDataTable/TileDataTable");
	public TileDataTable TileDataTable => m_tileDataTable;

	private PuzzleDataTable m_puzzleDataTable = new("DB/GameDataTable/PuzzleDataTable");
	public PuzzleDataTable PuzzleDataTable => m_puzzleDataTable;

	private RewardDataTable m_rewardDataTable = new("DB/GameDataTable/RewardDataTable");
	public RewardDataTable RewardDataTable => m_rewardDataTable;

	private ItemDataTable m_itemDataTable = new("DB/GameDataTable/ItemDataTable");
	public ItemDataTable ItemDataTable => m_itemDataTable;

	private ProductDataTable m_productDataTable = new("DB/GameDataTable/ProductDataTable");
	public ProductDataTable ProductDataTable => m_productDataTable;

    public int LastLevel => m_gameConfig.LastLevel;
    private GameConfig m_gameConfig = new(1, 3);

    [Serializable]
	public record GameConfig(int LastLevel, int RequiredMultiples)
	{
		public static GameConfig Init = new(LastLevel: 1, RequiredMultiples: 3);
	}

    public TableManager()
    {
        Initialize();
    }

    private void Initialize()
    {
        LoadGameConfig();
        m_puzzleDataTable.SetLastLevel(LastLevel);
    }

	public async UniTask<bool> LoadGameData()
	{
        Debug.Log(CodeManager.GetAsyncName());

        CountryCodeDataTable.Load();
		TileDataTable.Load();
		PuzzleDataTable.Load();
		RewardDataTable.Load();
		ItemDataTable.Load();
		ProductDataTable.Load();

		TextAsset[] piecesDataAssets = Resources.LoadAll<TextAsset>("DB/GameDataTable/PuzzlePieceDatas");
		await UniTask.Defer(() => PuzzleDataTable.LoadAsync(piecesDataAssets.Select(asset => asset.text).ToArray()));

		return true;
	}

	public async UniTask<bool> LoadLevelData(bool editorMode)
	{
        string path = PlayerPrefs.GetString(Constant.Editor.DATA_PATH_KEY);

		if (editorMode && PlayerPrefs.HasKey(Constant.Editor.LATEST_LEVEL_KEY))
		{
			int level = PlayerPrefs.GetInt(Constant.Editor.LATEST_LEVEL_KEY);

			var allFile = Directory.GetFiles(path, "*.json");

			string[] files = allFile?.Where(path => !path.Contains("_Temp"))?.ToArray();
			string[] temporaryFiles = allFile?.Where(path => path.Contains("_Temp"))?.ToArray();
            
			if (files?.Length > 0)
			{
				List<string> datas = new();

				foreach (var file in files)
				{
					if (file.StartsWith("Game"))
                    {
						continue;
                    }

					using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
					{
						using (StreamReader reader = new StreamReader(stream))
						{
							string data = await UniTask.Defer(() => reader.ReadToEndAsync().AsUniTask());

							datas.Add(data);
						}
					}
				}

				if (temporaryFiles?.Count() > 0)
				{
					foreach (var file in temporaryFiles)
					{
						using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
						{
							using (StreamReader reader = new StreamReader(stream))
							{
								string data = await UniTask.Defer(() => reader.ReadToEndAsync().AsUniTask());
								datas.Add(data);
							}
						}
					}
				}

				await LevelDataTable.LoadAsync(datas.ToArray());

				return true;
			}
		}

		TextAsset[] levelDataAssets = Resources.LoadAll<TextAsset>("DB/LevelDatas");
		await UniTask.Defer(() => LevelDataTable.LoadAsync(levelDataAssets.Select(asset => asset.text).ToArray()));

        return true;
	}

    private void LoadGameConfig()
    {
        TextAsset config = Resources.Load<TextAsset>("DB/LevelDatas/GameConfig");
        string json = config.text;
        m_gameConfig = LoadJson(json, () => GameConfig.Init) ?? GameConfig.Init;

        Debug.Log(CodeManager.GetAsyncName() + string.Format("<color=yellow>Last Level : {0}</color>", m_gameConfig.LastLevel));
    }

    private T LoadJson<T>(string jsonString, Func<T> onCreateNew)
	{
		try
		{
			return JsonConvert.DeserializeObject<T>(jsonString);
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			return onCreateNew.Invoke();
		}
	}
}
