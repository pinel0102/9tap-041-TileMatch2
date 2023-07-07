#nullable enable

using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using Newtonsoft.Json.UnityConverters.Math;

using Dropbox.Api;
using Dropbox.Api.Files;

public class LevelDataManager : IDisposable
{
	[Serializable]
	public record GameConfig(int LastLevel, int RequiredMultiples)
	{
		public static GameConfig Init = new(LastLevel: 1, RequiredMultiples: 3);
	}

	private const string DROPBOX_ACCESS_TOKEN = "sl.BhvQV-a7kjO5adkzvzSWunV5rUSLvBuVvCQ8yIrDr9k70w4hBqnaiIdrpF-iLgv5Yscuo7uaOGHOctOUMPbGdZX4RTFHP5CgkVtqtpaBDja0TLFw8nXzChFZbp7TVqSfX93kdHKUbZGu";

	private readonly JsonSerializerSettings m_serializerSettings;
	private readonly Dictionary<int, LevelData> m_savedLevelDataDic;
	private readonly string m_dataPath;
	private readonly CancellationTokenSource m_cancellationTokenSource;
	private readonly DropboxClient m_dropBox;

	private string GetLevelDataFileName(int level) => $"LevelData_{level}.dat";
	private string GetGameConfigFileName() => "GameConfig.dat";

	private string GetUrl(string fileName) => $"/{m_dataPath}/{fileName}";

	private GameConfig m_gameConfig = new(1, 3);
	private LevelData? m_currentData;

	public LevelData? CurrentLevelData => m_currentData;
	public GameConfig Config => m_gameConfig;

	public LevelDataManager(string dataPath)
	{
		m_dropBox = new DropboxClient(DROPBOX_ACCESS_TOKEN);
		m_cancellationTokenSource = new();
		m_dataPath = dataPath;
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
		m_dropBox?.Dispose();
		m_cancellationTokenSource?.Dispose();
	}

	public async UniTask<LevelData?> LoadConfig()
	{
		var list = await m_dropBox.Files.ListFolderAsync(string.Empty);
		string fileName = GetGameConfigFileName();
		GameConfig newConfig = GameConfig.Init;
		
		m_gameConfig = await LoadInternal<GameConfig>(fileName, () => GameConfig.Init) ?? GameConfig.Init;
		await SaveInternal(fileName, newConfig);
		return await LoadLevelDataInternal(1);
	}

	public async UniTask<LevelData?> CreateLevelData()
	{
		int nextLevel = m_gameConfig.LastLevel + 1;
		m_gameConfig = m_gameConfig with { LastLevel = nextLevel };
		return await LoadLevelDataInternal(nextLevel);
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
		int selectLevel = Mathf.Max((m_currentData?.Level + direction) ?? 1, 1);
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

	public UniTask SaveLevelData()
	{
		if (m_currentData == null)
		{
			return UniTask.CompletedTask;
		}

		int level = m_currentData.Level;
		string path = GetLevelDataFileName(level);

		return UniTask.WhenAll(
			SaveInternal(GetGameConfigFileName(), m_gameConfig),
			UniTask.Create(
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
			)
		);
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

	public int? UpdateNumberOfTypes(int number)
	{
		if (m_currentData == null)
		{
			return default;
		}

		int clamp = Mathf.Max(number, 1);

		m_currentData = m_currentData with {
			NumberOfTileTypes = clamp
		};

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

	private async UniTask<T?> LoadInternal<T>(string fileName, Func<T> onCreateNew)
	{
		var list = await m_dropBox.Files
			.ListFolderAsync(new ListFolderArg($"/{m_dataPath}"))
			.AsUniTask()
			.AttachExternalCancellation(m_cancellationTokenSource.Token);
		
		if (list.Entries.All(entry => !entry.IsFile || entry.Name != fileName))
		{
			await SaveInternal(fileName, onCreateNew.Invoke());
		}

		var res = await m_dropBox.Files
			.DownloadAsync(new DownloadArg(GetUrl(fileName)))
			.AsUniTask()
			.AttachExternalCancellation(m_cancellationTokenSource.Token);

		string json = await res.GetContentAsStringAsync();
		return JsonConvert.DeserializeObject<T>(json, m_serializerSettings);
	}

	private async UniTask SaveInternal<T>(string fileName, T data)
	{
		if (data == null)
		{
			return;
		}

		string json = JsonConvert.SerializeObject(data, m_serializerSettings);
		
		using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
		{
			await m_dropBox.Files
				.UploadAsync(GetUrl(fileName), WriteMode.Overwrite.Instance, body: ms)
				.AsUniTask()
				.AttachExternalCancellation(m_cancellationTokenSource.Token);
		}
	}
}
