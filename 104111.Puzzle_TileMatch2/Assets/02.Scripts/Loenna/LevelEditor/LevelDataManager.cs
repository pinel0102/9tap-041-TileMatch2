#nullable enable

using UnityEngine;
using UnityEngine.Assertions;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

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

	public record CachedLevelData(bool Saved, LevelData LevelData);

    private readonly LevelEditor levelEditor;
	private readonly JsonSerializerSettings m_serializerSettings;
	private readonly Dictionary<int, CachedLevelData> m_cachedLevelDataDic;
	private readonly CancellationTokenSource m_cancellationTokenSource;

	private string m_folderPath = string.Empty;
	private string GetLevelDataFileName(int level) => $"LevelData_{level}.json";
	private string GetGameConfigFileName() => "GameConfig.json";

	private GameConfig m_gameConfig = new(1, 3);
	private LevelData? m_currentData;

	public LevelData? CurrentLevelData => m_currentData;
	public GameConfig Config => m_gameConfig;

	public AsyncReactiveProperty<bool> m_saved = new(false);
	public IReadOnlyAsyncReactiveProperty<bool> Saved => m_saved;

	public int GetLastLayerInCurrent(int boardIndex) => (m_currentData?.Boards?[boardIndex]?.Layers?.Count - 1) ?? 0;

	public LevelDataManager(LevelEditor editor)
	{
        levelEditor = editor;
		m_cancellationTokenSource = new();
		m_cachedLevelDataDic = new();
		m_serializerSettings = new JsonSerializerSettings {
			Converters = new [] {
				new Vector3Converter()
			},
			Formatting = Formatting.Indented,
			ContractResolver = new UnityTypeContractResolver()
		};

		Saved.WithoutCurrent().Subscribe(
			saved => {
				if (m_currentData != null)
				{
					if (m_cachedLevelDataDic.TryGetValue(m_currentData.Key, out var cachedLevelData))
					{
						m_cachedLevelDataDic[m_currentData.Key] = cachedLevelData with { Saved = saved };
					}
				}
			}
		);
	}

	public void Dispose()
	{
		m_cancellationTokenSource?.Dispose();
	}

	public async UniTask<LevelData?> LoadConfig(string path)
	{
		Assert.IsFalse(string.IsNullOrWhiteSpace(path));
		
		m_currentData = null;
		m_cachedLevelDataDic.Clear();
		m_folderPath = path;

		string fileName = GetGameConfigFileName();
		
		m_gameConfig = await LoadInternal(fileName, () => GameConfig.Init) ?? GameConfig.Init;
		await SaveInternal(fileName, m_gameConfig);

		string key = Constant.Editor.LATEST_LEVEL_KEY;

		if (!PlayerPrefs.HasKey(key))
		{
			PlayerPrefs.SetInt(key, 1);
		}

		int level = PlayerPrefs.GetInt(key);

		return await LoadLevelDataInternal(level, false);
	}

	public async UniTask<LevelData?> LoadLevelData(int level, bool forceLoad)
	{
		if (level < 0)
		{
			return m_currentData;
		}

		return await LoadLevelDataInternal(level, forceLoad);
	}

	public UniTask<LevelData?> LoadLevelDataByStep(int direction)
	{
		int selectLevel = Mathf.Max((m_currentData?.Key + direction) ?? 1, 1);
		return LoadLevelDataInternal(selectLevel, false);
	}

	private async UniTask<LevelData?> LoadLevelDataInternal(int level, bool forceLoad)
	{
		string fileName = GetLevelDataFileName(level);
		string temporaryFileName = $"LevelData_{level}_Temp.json";

        (bool saved, var data) = File.Exists(Path.Combine(m_folderPath, temporaryFileName))?
			(
				false, 
				await LoadInternal(temporaryFileName, () => LevelData.CreateData(level))
			):
			(
				true, 
				m_cachedLevelDataDic.ContainsKey(level) && !forceLoad? 
				m_cachedLevelDataDic[level].LevelData :
				await LoadInternal(fileName, () => LevelData.CreateData(level))
			);
		
		if (data == null)
		{
			Debug.LogException(new NullReferenceException(nameof(data)));
			return default;
		}

		if (m_cachedLevelDataDic.TryGetValue(level, out var cachedData))
		{
			m_cachedLevelDataDic[level] = cachedData with { LevelData = data };
			saved = cachedData.Saved;
		}
		else
		{
			m_cachedLevelDataDic.Add(level, new CachedLevelData(saved, data));
		}

		m_currentData = data;
		m_saved.Value = forceLoad? true : saved;

		return data;
	}

	public async UniTask SaveTemporaryLevelData()
	{
		if (m_saved.Value)
		{
			return;
		}

		if (m_currentData == null)
		{
			return;
		}

		int level = m_currentData.Key;
		RemoveTemporaryLevelData(level);
		string path = $"LevelData_{level}_Temp.json";

		await SaveInternal(path, m_currentData);
	}

	public async UniTask SaveLevelData(bool showLog = true)
	{
		if (m_currentData == null)
		{
			return;
		}

		int level = m_currentData.Key;
		string path = GetLevelDataFileName(level);
		RemoveTemporaryLevelData(level);

        await UniTask.Create(
			async () => {
				await SaveInternal(GetLevelDataFileName(level), m_currentData);

				if (m_cachedLevelDataDic.TryGetValue(level, out var cachedData))
				{
					m_cachedLevelDataDic[level] = cachedData with { Saved = true, LevelData = m_currentData };
				}
				else
				{
					m_cachedLevelDataDic.Add(level, new CachedLevelData(true, m_currentData));
				}
			}
		);

		m_gameConfig = m_gameConfig with { LastLevel = Mathf.Max(m_gameConfig.LastLevel, level) };

		await SaveInternal(GetGameConfigFileName(), m_gameConfig);

		m_saved.Value = true;

        if (showLog)
            levelEditor.SetLog(string.Format("Save Level {0}", level));
	}

	public void AddBoardData(out int count)
	{
		if (m_currentData?.Boards == null)
		{
			count = m_currentData?.Boards?.Count ?? 0;
			return;
		}
        List<Layer> layers = new()
        {
            new Layer()
        };

        m_currentData.Boards.Add(new Board(layers));

		count = m_currentData.Boards.Count;

		m_saved.Value = false;
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

		m_saved.Value = false;
		return true;
	}

    public bool TryCopyBoardData(int boardIndex, out int toLevel, out Board? boardToCopy)
	{
		if (!m_currentData?.Boards?.HasIndex(boardIndex) ?? false)
		{
            levelEditor.SetLog(string.Format("Board [{0}] : Not Index", boardIndex), true);
			toLevel = -1;
            boardToCopy = null;
			return false;
		}

        if (m_currentData?.Boards?[boardIndex]?.IsEmptyBoard ?? true)
        {
            levelEditor.SetLog(string.Format("Board [{0}] : Board is Empty", boardIndex), true);
            toLevel = -1;
            boardToCopy = null;
            return false;
        }

        toLevel = Config.LastLevel + 1;
        boardToCopy = m_currentData.Boards[boardIndex];
        
        return true;
	}

	// index뒤에 추가
	public bool TryAddLayerData(int boardIndex, out int addedLayerIndex)
	{
		addedLayerIndex = -1;
		Board? board = m_currentData?[boardIndex];

		if (board == null)
		{
			return false;
		}

		addedLayerIndex = board.Layers.Count;
		board.Layers.Add(new Layer());

		m_saved.Value = false;
		return true;
	}

	public bool TryRemovePeekLayer(int boardIndex)
	{
		Board? board = m_currentData?[boardIndex];

		if (board?.Layers == null)
		{
			return false;
		}

		int last = board.Layers.Count - 1;

		if (last > 0)
		{
			board.Layers.RemoveAt(last);
			return true;
		}
		else
		if (last == 0)
		{
			board.Layers[0].Tiles.Clear();
			return true;
		}

		m_saved.Value = false;
		return false;
	}

	public bool TrySetMissionTile(int boardIndex, int layerIndex, Guid guid, bool attached)
	{
		if (m_currentData == null)
		{
			return false;
		}

		if (!m_currentData.Boards.HasIndex(boardIndex))
		{
			return false;
		}

		Board board = m_currentData[boardIndex]!;

		if (!board?.Layers?.HasIndex(layerIndex) ?? true)
		{
			return false;
		}

		Layer layer = board!.Layers[layerIndex];

		if (layer.Tiles == null)
		{
			return false;
		}

		if (layer.Tiles.FindIndex(tile => tile.Guid == guid) is int index and >= 0)
		{
			Tile data = layer.Tiles[index];
			layer.Tiles[index] = data with { IncludeMission = attached };
		}

		m_saved.Value = false;
		return true;
	}

    public int AddBlocker(int boardIndex, BlockerTypeEditor blockerTypeEditor, int count, int blockerICD)
    {
        if (m_currentData == null)
			return 0;
		
		if (!m_currentData.Boards.HasIndex(boardIndex))
			return 0;

        if(GlobalDefine.TryParseBlockerType(blockerTypeEditor, out List<BlockerType> blockerTypeList))
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0} x {1} (ICD : {2})</color>", blockerTypeEditor, count, blockerICD));

            GlobalDefine.InitRandomSeed();

            Board board = m_currentData[boardIndex]!;
            int successCount = 0;

            for(int k=0; k < count; k++)
            {
                for(int i=0; i < blockerTypeList.Count; i++)
                {
                    BlockerType blockerType = blockerTypeList[i];
                    //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>blockerType : {0}</color>", blockerType));
                    
                    List<Tile> targetList = new List<Tile>();

                    for (int index = board.Layers.Count - 1; index >= 0; index--)
                    {
                        Layer layer = board.Layers[index];
                        targetList.AddRange(layer.GetBlockerEnableTiles(blockerType));
                    }

                    if (targetList.Count > 0)
                    {
                        //Debug.Log(CodeManager.GetMethodName() + string.Format("Enable Count : {0}", targetList.Count));

                        Tile target = targetList.GetRandomTile();

                        //Debug.Log(CodeManager.GetMethodName() + string.Format("[Target] {0} / {1} / {2} / {3}", target.Guid, target.Blocker, target.BlockerICD, target.Position));

                        for (int layerIndex = board.Layers.Count - 1; layerIndex >= 0; layerIndex--)
                        {
                            Layer layer = board.Layers[layerIndex];
                            if (layer.Tiles?.FindIndex(tile => tile.Guid == target.Guid) is int index and >= 0)
                            {
                                Tile resultTile = layer.Tiles[index];
                                layer.Tiles[index] = resultTile with { Blocker = blockerType, BlockerICD = GlobalDefine.GetBlockerICD(blockerType, blockerICD) };

                                Debug.Log(CodeManager.GetMethodName() + string.Format("[Success] {0} / {1} / {2} / {3}", layer.Tiles[index].Guid, layer.Tiles[index].Blocker, layer.Tiles[index].BlockerICD, layer.Tiles[index].Position));

                                m_saved.Value = false;
                                successCount++;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning(CodeManager.GetMethodName() + string.Format("No Enable Target : {0}", blockerType));                        
                        return successCount;
                    }
                }
            }

            return successCount;
        }
        else
        {
            Debug.LogWarning(CodeManager.GetMethodName() + string.Format("<color=yellow>Parsing Error : {0}</color>", blockerTypeEditor));            
            return 0;
        }
    }

    public int ClearBlocker(int boardIndex, BlockerTypeEditor blockerTypeEditor)
    {
        if (m_currentData == null)
			return 0;
		
		if (!m_currentData.Boards.HasIndex(boardIndex))
			return 0;

        if(GlobalDefine.TryParseBlockerType(blockerTypeEditor, out List<BlockerType> blockerTypeList))
        {
            //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>{0}</color>", blockerTypeEditor));

            Board board = m_currentData[boardIndex]!;
            int removeCount = 0;

            for(int i=0; i < blockerTypeList.Count; i++)
            {
                BlockerType blockerType = blockerTypeList[i];

                //Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>blockerType : {0}</color>", blockerType));

                for (int index = board.Layers.Count - 1; index >= 0; index--)
                {
                    Layer layer = board.Layers[index];

                    for(int k=0; k < layer.Tiles.Count; k++)
                    {
                        if (layer.Tiles[k].Blocker.Equals(blockerType))
                        {
                            Tile target = layer.Tiles[k];

                            //Debug.Log(CodeManager.GetMethodName() + string.Format("[Target] {0} / {1} / {2} / {3}", target.Guid, target.Blocker, target.BlockerICD, target.Position));

                            layer.Tiles[k] = target with { Blocker = BlockerType.None, BlockerICD = 0 };
                            
                            Debug.Log(CodeManager.GetMethodName() + string.Format("[Success] {0} / {1} / {2} / {3}", layer.Tiles[k].Guid, layer.Tiles[k].Blocker, layer.Tiles[k].BlockerICD, layer.Tiles[k].Position));

                            m_saved.Value = false;
                            removeCount++;
                        }
                    }
                }
            }

            return removeCount;
        }
        else
        {
            Debug.LogWarning(CodeManager.GetMethodName() + string.Format("<color=yellow>Parsing Error : {0}</color>", blockerTypeEditor));
            return 0;
        }
    }

    /*public bool TrySetBlockerTile(int boardIndex, int layerIndex, Guid guid, BlockerType blockerType, int variableICD)
	{
		if (m_currentData == null)
		{
			return false;
		}

		if (!m_currentData.Boards.HasIndex(boardIndex))
		{
			return false;
		}

		Board board = m_currentData[boardIndex]!;

		if (!board?.Layers?.HasIndex(layerIndex) ?? true)
		{
			return false;
		}

		Layer layer = board!.Layers[layerIndex];

		if (layer.Tiles == null)
		{
			return false;
		}

		if (layer.Tiles.FindIndex(tile => tile.Guid == guid) is int index and >= 0)
		{
			Tile data = layer.Tiles[index];
            return TrySetBlockerTile(data, blockerType, variableICD);
		}

        return false;
	}

    public bool TrySetBlockerTile(Tile tile, BlockerType blockerType, int variableICD)
	{
		if (tile == null)
		{
			return false;
		}

        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[From][{0}] {1} (ICD : {2})</color>", tile.Guid, tile.Blocker, tile.BlockerICD));

		tile = tile with { Blocker = blockerType, BlockerICD = GlobalDefine.GetBlockerICD(blockerType, variableICD) };
            
        Debug.Log(CodeManager.GetMethodName() + string.Format("<color=yellow>[==To][{0}] {1} (ICD : {2})</color>", tile.Guid, tile.Blocker, tile.BlockerICD));

		m_saved.Value = false;
		return true;
	}*/

	public bool TryAddTileData(DrawOrder drawOrder, int boardIndex, Bounds bounds, out int layerIndex)
	{
		layerIndex = -1;

		if (m_currentData == null)
		{
			return false;
		}

		if (!m_currentData.Boards.HasIndex(boardIndex))
		{
			return false;
		}

		Board board = m_currentData[boardIndex]!;

		if (board.Layers == null)
		{
			board.Layers = new();
		}

		switch (drawOrder)
		{
			case DrawOrder.BOTTOM:
				for (int index = 0, count = board.Layers.Count; index < count; index++)
				{
					if (!board.Layers.HasIndex(index))
					{
						continue;
					}

					Layer layer = board.Layers[index];
					if (layer.Tiles?.All(tile => Intersect(tile.GetBounds(bounds.size.x), bounds)) ?? false)
					{
						return AddTileInLayer(bounds.center, out layerIndex, index, layer);
					}

				}

				if (TryAddLayerData(boardIndex, out int addIndex))
				{
					return AddTileInLayer(bounds.center, out layerIndex, addIndex, board.Layers[addIndex]);
				}
				break;
			case DrawOrder.TOP:
				int lastIndex = board.Layers.Count - 1;
				for (int index = lastIndex; index >= 0; index--)
				{
					if (!board.Layers.HasIndex(index))
					{
						continue;
					}

					Layer layer = board.Layers[index];
					if (layer.Tiles?.Any(tile => !Intersect(tile.GetBounds(bounds.size.x), bounds)) ?? false)
					{
						if (index >= lastIndex)
						{
							if (TryAddLayerData(boardIndex, out int addTopIndex))
							{
								return AddTileInLayer(bounds.center, out layerIndex, addTopIndex, board.Layers[addTopIndex]);
							}
						}
						else
						{
							int upperIndex = index + 1;
							return AddTileInLayer(bounds.center, out layerIndex, upperIndex, board.Layers[upperIndex]);
						}
					}
				}
				return AddTileInLayer(bounds.center, out layerIndex, 0, board.Layers[0]);
		}
		return true;

		bool AddTileInLayer(Vector2 position, out int layerIndex, int index, Layer layer)
		{
            Debug.Log(position);
			layer.Tiles.Add(new Tile(position));
			layerIndex = index;
			m_saved.Value = false;
			return true;
		}

		
		bool Intersect(Bounds placed, Bounds bounds)
		{
			return placed.SqrDistance(bounds.center) >= Mathf.Pow(bounds.size.x * 0.5f, 2f);
		}
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

		m_saved.Value = false;
		return clamp;
	}

	public bool TryRemoveTileData(int boardIndex, Bounds bounds)
	{
		if (m_currentData == null)
		{
			return false;
		}

		if (!m_currentData.Boards.HasIndex(boardIndex))
		{
			return false;
		}

		Board board = m_currentData[boardIndex]!;

		if (board.Layers == null)
		{
			return false;
		}
		
		for (int index = board.Layers.Count - 1; index >= 0; index--)
		{
			Layer layer = board.Layers[index];

			int removeCount = layer.Tiles.RemoveAll(tile => Overlap(bounds, tile.GetBounds(bounds.size.x)));
			if (removeCount > 0)
			{
				if (layer.Tiles.Count <= 0 && board.Layers.Count > 1)
				{
					board.Layers.RemoveAt(index);
				}

				m_saved.Value = false;
				return true;
			}
		}

		return false;

		bool Overlap(Bounds bounds, Bounds other)
		{
			return bounds.min.x >= other.min.x &&
				bounds.min.y >= other.min.y &&
				bounds.max.x <= other.max.x &&
				bounds.max.y <= other.max.y;
		}
	}

	public int? SetDifficult(int boardIndex, int difficult)
	{
		if (m_currentData == null)
		{
			return default;
		}

		Board board = m_currentData[boardIndex]!;
		board.Difficult = difficult;

		m_currentData.Boards[boardIndex] = board;

		m_saved.Value = false;
		return difficult;
	}

	public bool SetHardMode(bool hardMode)
	{
		if (m_currentData == null)
		{
			return false;
		}

		m_currentData = m_currentData with { HardMode = hardMode };

		m_saved.Value = false;
		return m_currentData.HardMode;
	}

    public async UniTask<bool> TrySwapLevel(int fromLevel, int toLevel)
    {
        if (m_currentData == null)
			return false;
		
        try {
            string fromFile = GetLevelDataFileName(fromLevel);
            string toFile = GetLevelDataFileName(toLevel);
            string fromPath = Path.Combine(m_folderPath, fromFile);
            string toPath = Path.Combine(m_folderPath, toFile);

            if (File.Exists(fromPath) && File.Exists(toPath))
            {
                LevelData fromData = m_currentData with { Key = toLevel};

                await LoadLevelData(toLevel, true);

                LevelData toData = m_currentData with { Key = fromLevel};
                
                await SaveInternal(toFile, fromData);
                await SaveInternal(fromFile, toData);
                await LoadLevelDataInternal(toLevel, true);
                await LoadLevelDataInternal(fromLevel, true);

                levelEditor.SetLog(string.Format("Swap Level {0} <-> {1}", fromLevel, toLevel));

                return true;
            }
            else
            {
                levelEditor.SetLog(string.Format("Swap Level Failed : {0} Exists:{1} <-> {2} Exists:{3}", fromLevel, File.Exists(fromPath), toLevel, File.Exists(toPath)), true);
            }
        }
        catch(Exception ex)
        {
            levelEditor.SetLog(ex.ToString(), true);
        }

        return false;
    }

	private async UniTask<T?> LoadInternal<T>(string fileName, Func<T> onCreateNew)
	{
		try
		{
			string path = Path.Combine(m_folderPath, fileName);
            
            //Debug.Log(CodeManager.GetMethodName() + path);
			
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
        string backupFolder = Path.Combine(m_folderPath, "Backup");
        string backupPath = Path.Combine(backupFolder, fileName);

        //Debug.Log(backupPath);

        if (!Directory.Exists(backupFolder))
            Directory.CreateDirectory(backupFolder);

		if (File.Exists(path))
		{
			if (File.Exists($"{backupPath}.backup"))
			{
				File.Delete($"{backupPath}.backup");
			}
			File.Move(path, $"{backupPath}.backup");
		}

		using (FileStream fileStream = new FileStream(path: path, access: FileAccess.Write, mode: FileMode.OpenOrCreate))
		{
			using (StreamWriter writer = new StreamWriter(fileStream))
			{
				await writer.WriteAsync(json).AsUniTask().AttachExternalCancellation(m_cancellationTokenSource.Token);
			}
		}
	}

	public void UpdateCountryCode(string code)
	{
		if (m_currentData == null)
		{
			return;
		}

		m_currentData = m_currentData with { CountryCode = code };
		m_saved.Value = false;
	}

    public void RemoveTemporaryLevelData(int level)
    {
		string temporaryFileName = $"LevelData_{level}_Temp.json";
		string path = Path.Combine(m_folderPath, temporaryFileName);

		if (File.Exists(path))
		{
			File.Delete(path);
		}

		m_saved.Value = false;
    }

	public int? UpdateMissionTileCount(int boardIndex, int value)
	{
		if (m_currentData == null)
		{
			return default;
		}

		Board board = m_currentData[boardIndex]!;
		board.MissionTileCount = value;

		m_currentData.Boards[boardIndex] = board;

		m_saved.Value = false;
		return value;
	}

	public void UpdateMissionTileIcon(int boardIndex, int icon)
	{
		if (m_currentData == null)
		{
			return;
		}

		Board board = m_currentData[boardIndex]!;
		board.GoldTileIcon = icon;

		m_currentData.Boards[boardIndex] = board;

		m_saved.Value = false;
	}
}
