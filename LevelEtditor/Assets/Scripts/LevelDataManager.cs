#nullable enable

//#define USING_GOOGLE_DRIVE

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

#if USING_GOOGLE_DRIVE
using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Download;
using Google.Apis.Upload;

using Data = Google.Apis.Drive.v3.Data;
using static Google.Apis.Drive.v3.FilesResource;
#endif

public class LevelDataManager : IDisposable
{

#if USING_GOOGLE_DRIVE
	private const string DRIVE_FOLDER_ID = "1I6kpVvTneQxZUlsINeAZ_6t9By56ziis";
	private const string CONTENT_TYPE = "application/json";
#endif

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
#if USING_GOOGLE_DRIVE
		UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
			new ClientSecrets
			{
				ClientId = "350287200496-onh5jtj9e5bbc8a61qasgugrrm32cfph.apps.googleusercontent.com",
				ClientSecret = "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"
			},
			new[] { DriveService.Scope.Drive },
			"user",
			m_cancellationTokenSource.Token,
			new FileDataStore("Drive.TileMatch2Data")
		);
		
		m_driveService = new DriveService(
			new BaseClientService.Initializer(){
				HttpClientInitializer = credential,
				ApplicationName = "Level_Editor"
			}
		);
#endif

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
		string path = Path.Combine(m_folderPath, fileName);

#if USING_GOOGLE_DRIVE
		if (m_driveService == null)
		{
			return default(T);
		}

		Data.File? file = await GetFileInDrive(fileName);

		if (file == null)
		{
			return onCreateNew.Invoke();
		}

		var request = m_driveService.Files.Get(file.Id);
		IDownloadProgress result;

		using (FileStream fileStream = new FileStream(path: path, mode: FileMode.OpenOrCreate, access: FileAccess.ReadWrite))
		{
			UniTaskCompletionSource<IDownloadProgress> downloadProgress = new();

			downloadProgress.TrySetResult(
				await request.DownloadAsync(fileStream, m_cancellationTokenSource.Token)
				.AsUniTask()
			);

			result = await downloadProgress.Task;
		}

		if (result.Status == DownloadStatus.Completed)
		{
			string json = System.IO.File.ReadAllText(path);
			return JsonConvert.DeserializeObject<T>(json, m_serializerSettings);
		}
		
		return default(T);
#else
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
#endif
	}

	private async UniTask SaveInternal<T>(string fileName, T data)
	{
		if (data == null)
		{
			return;
		}

		string json = JsonConvert.SerializeObject(data, m_serializerSettings);

#if USING_GOOGLE_DRIVE
		if (m_driveService == null)
		{
			return;
		}

		var metadata = new Data.File { Name = fileName };
		
		using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
		{
            ResumableUpload<Data.File, Data.File> request = await MakeRequest();

			UniTaskCompletionSource<IUploadProgress> uploadProgress = new();
			uploadProgress.TrySetResult(
				await request
				.UploadAsync(m_cancellationTokenSource.Token)
				.AsUniTask()
			);

			var result = await uploadProgress.Task;

			if (result.Status == UploadStatus.Failed)
			{
				Debug.LogError(result.Exception);
			}

			async UniTask<ResumableUpload<Data.File, Data.File>> MakeRequest()
			{
				var file = await GetFileInDrive(fileName);

				if (file != null)
				{
					UpdateMediaUpload update = m_driveService.Files.Update(metadata, file.Id, ms, CONTENT_TYPE);
					update.AddParents = DRIVE_FOLDER_ID;
				
					return update;
				}
				else
				{
					metadata.Parents = new List<string> { DRIVE_FOLDER_ID };
					CreateMediaUpload create = m_driveService.Files.Create(metadata, ms, CONTENT_TYPE);
					create.UploadType = "resumable";
					
					return create;
				}
			}
		}
#else
		string path = Path.Combine(m_folderPath, fileName);
		using (FileStream fileStream = new FileStream(path: path, access: FileAccess.Write, mode: FileMode.OpenOrCreate))
		{
			using (StreamWriter writer = new StreamWriter(fileStream))
			{
				await writer.WriteAsync(json).AsUniTask().AttachExternalCancellation(m_cancellationTokenSource.Token);
			}
		}
#endif
	}

#if USING_GOOGLE_DRIVE
	private async UniTask<Data.File?> GetFileInDrive(string fileName)
	{
		if (m_driveService == null)
		{
			return default;
		}

		var fileListRequest = m_driveService.Files.List();

		//fileListRequest.Q = $"name = '{fileName}' and trashed = false";
		//fileListRequest.IncludeItemsFromAllDrives = true;

		var list = await fileListRequest.ExecuteAsync(m_cancellationTokenSource.Token).AsUniTask();

		return list.Files.FirstOrDefault(file => file.Name == fileName);
	}
#endif
}
