#nullable enable

using UnityEngine;
using UnityEngine.Assertions;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using Newtonsoft.Json.UnityConverters.Math;

public class LevelDataManager : IDisposable
{
	[Serializable]
	public record GameConfig(int LastLevel, int RequiredMultiples)
	{
		public static GameConfig Init = new(LastLevel: 1, RequiredMultiples: 3);
	}

	private readonly JsonSerializerSettings m_serializerSettings;
	private readonly Dictionary<int, LevelData> m_savedLevelDataDic;
	private readonly CancellationTokenSource m_cancellationTokenSource;

	private readonly string m_folderPath;
	private string GetLevelDataFileName(int level) => $"LevelData_{level}.json";
	private string GetGameConfigFileName() => "GameConfig.json";

#if USING_GOOGLE_DRIVE
	private DriveService? m_driveService;
#endif
	private GameConfig m_gameConfig = new(1, 3);
	private LevelData? m_currentData;

	public LevelData? CurrentLevelData => m_currentData;
	public GameConfig Config => m_gameConfig;

	public Dictionary<int, LevelData> CachedLevelDataDic
	{
		get
		{
			var enumerable = m_savedLevelDataDic
			.Select(
				pair =>
				{
					if (m_currentData != null && m_savedLevelDataDic.ContainsKey(m_currentData.Key))
					{
						return new KeyValuePair<int, LevelData>(m_currentData.Key, m_currentData);
					}

					return pair;
				}
			);

			var dic = new Dictionary<int, LevelData>();
			foreach(var (key, value) in enumerable)
            {
				dic.TryAdd(key, value);
            }

			if (m_currentData != null && !dic.ContainsKey(m_currentData.Key))
			{
				dic.TryAdd(m_currentData.Key, m_currentData);
			}

			return dic;
		}
	}

	public LevelDataManager(string path)
	{
		Assert.IsFalse(string.IsNullOrWhiteSpace(path));

		m_folderPath = path;
		m_cancellationTokenSource = new();
		m_savedLevelDataDic = new();
		m_serializerSettings = new JsonSerializerSettings {
			Converters = new [] {
				new Vector3Converter()
			},
			Formatting = Formatting.Indented,
			ContractResolver = new UnityTypeContractResolver()
		};
	}

	public void Dispose()
	{
		m_cancellationTokenSource?.Dispose();
	}

	public async UniTask<LevelData?> LoadConfig()
	{
		string fileName = GetGameConfigFileName();
		GameConfig newConfig = GameConfig.Init;
		
		m_gameConfig = await LoadInternal<GameConfig>(fileName, () => GameConfig.Init) ?? GameConfig.Init;
		await SaveInternal(fileName, newConfig);
		return await LoadLevelDataInternal(1);
	}

	public async UniTask<LevelData?> LoadLevelData(int level)
	{
		if (level < 0 || level >= m_gameConfig.LastLevel)
		{
			return m_currentData;
		}

		return await LoadLevelDataInternal(level);
	}

	public UniTask<LevelData?> LoadLevelDataByStep(int direction)
	{
		int selectLevel = Mathf.Max((m_currentData?.Key + direction) ?? 1, 1);
		return LoadLevelDataInternal(selectLevel);
	}

	private async UniTask<LevelData?> LoadLevelDataInternal(int level)
	{
		string fileName = GetLevelDataFileName(level);

		var data = m_savedLevelDataDic.ContainsKey(level)? 
			m_savedLevelDataDic[level] :
			await LoadInternal<LevelData>(fileName, () => LevelData.CreateData(level));
		
		if (data == null)
		{
			Debug.LogException(new NullReferenceException(nameof(data)));
			return default;
		}

		if (!m_savedLevelDataDic.ContainsKey(level))
		{
			m_savedLevelDataDic.Add(level, data);
		}
		else
		{
			m_savedLevelDataDic[level] = data;
		}

		m_currentData = data;

		return data;
	}

	public async UniTask SaveLevelData()
	{
		if (m_currentData == null)
		{
			return;
		}

		int level = m_currentData.Key;
		string path = GetLevelDataFileName(level);

		await UniTask.Create(
			async () => {
				await SaveInternal(GetLevelDataFileName(level), m_currentData);
				if (!m_savedLevelDataDic.ContainsKey(level))
				{
					m_savedLevelDataDic.Add(level, m_currentData);
				}
				else
				{
					m_savedLevelDataDic[level] = m_currentData;
				}
			}
		);

		m_gameConfig = m_gameConfig with { LastLevel = Mathf.Max(m_gameConfig.LastLevel, level) };

		await SaveInternal(GetGameConfigFileName(), m_gameConfig);
	}

	public void AddBoardData(out int count)
	{
		if (m_currentData?.Boards == null)
		{
			count = m_currentData?.Boards?.Count ?? 0;
			return;
		}
		List<Layer> layers = new();
		layers.Add(new Layer());

		m_currentData.Boards.Add(new Board(layers));

		count = m_currentData.Boards.Count;
	}

	public bool TryRemoveBoardData(int boardIndex, out int boardCount)
	{
		if (!m_currentData?.Boards?.HasIndex(boardIndex) ?? false)
		{
			boardCount = m_currentData?.Boards?.Count ?? 0;
			return false;
		}
		
		m_currentData?.Boards.RemoveAt(boardIndex);
		boardCount = m_currentData?.Boards?.Count ?? 0;
		return true;
	}

	// index뒤에 추가
	public bool TryAddLayerData(int boardIndex, int prevIndex, out int addIndex)
	{
		Board? board = m_currentData?[boardIndex];

		if (board?.Layers.HasIndex(prevIndex) ?? false)
		{
			addIndex = prevIndex + 1;
			board.Layers.Insert(addIndex, new Layer());
			return true;
		}

		addIndex = prevIndex;
		return false;
	}

	public void RemoveLayerData(int boardIndex, int index)
	{
		Board? board = m_currentData?[boardIndex];

		if (board?.Layers.HasIndex(index) ?? false)
		{
			board.Layers.RemoveAt(index);
		}
	}

	public bool TryAddTileData(int boardIndex, int layerIndex, Vector2 position)
	{
		Layer? layerData = m_currentData?.GetLayer(boardIndex, layerIndex);

		if (layerData?.Tiles?.All(tile => !position.Equals(tile.Position)) ?? false)
		{
			layerData.Tiles.Add(new Tile(0, position));
			return true;
		}

		return false;
	}

	public int? UpdateNumberOfTypes(int boardIndex, int number)
	{
		if (m_currentData == null)
		{
			return default;
		}

		if (m_currentData?.Boards?.Count <= 0)
		{
			return default;
		}

		int clamp = Mathf.Max(number, 1);

		m_currentData![boardIndex]!.NumberOfTileTypes = clamp;

		return clamp;
	}

	public bool TryRemoveTileData(int boardIndex, int layerIndex, Vector2 position)
	{
		Layer? layerData = m_currentData?.GetLayer(boardIndex, layerIndex);

		if (layerData?.Tiles is var tiles and not null)
		{
			return tiles.RemoveAll(tile => tile.Position == position) > 0;
		}

		return false;
	}

	public void ClearTileDatasInLayer(int boardIndex, int layerIndex)
	{
		Layer? layerData = m_currentData?[boardIndex]?[layerIndex];

		if (layerData != null)
		{
			layerData.Tiles.Clear();
		}
	}

	public int? SetDifficult(int difficult)
	{
		if (m_currentData == null)
		{
			return default;
		}

		m_currentData = m_currentData with {
			Difficult = difficult
		};

		return difficult;
	}

	private async UniTask<T?> LoadInternal<T>(string fileName, Func<T> onCreateNew)
	{
		try
		{
			string path = Path.Combine(m_folderPath, fileName);
			if (!File.Exists(path))
			{
				return onCreateNew.Invoke();
			}

			using (FileStream fileStream = new FileStream(path: path, mode: FileMode.Open, access: FileAccess.Read))
			{
				using (StreamReader reader = new StreamReader(fileStream))
				{
					UniTaskCompletionSource<string> source = new();
					source.TrySetResult(await reader.ReadToEndAsync());

					string json = await source.Task;

					return JsonConvert.DeserializeObject<T>(json, m_serializerSettings);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());

			return onCreateNew.Invoke();
		}
	}

	private async UniTask SaveInternal<T>(string fileName, T data)
	{
		if (data == null)
		{
			return;
		}

		string json = JsonConvert.SerializeObject(data, m_serializerSettings);
		string path = Path.Combine(m_folderPath, fileName);
		using (FileStream fileStream = new FileStream(path: path, access: FileAccess.Write, mode: FileMode.OpenOrCreate))
		{
			using (StreamWriter writer = new StreamWriter(fileStream))
			{
				await writer.WriteAsync(json).AsUniTask().AttachExternalCancellation(m_cancellationTokenSource.Token);
			}
		}
	}
}
