#nullable enable

using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using Newtonsoft.Json.UnityConverters.Math;

public class LevelDataManager
{
	[Serializable]
	public record GameConfig(int LastLevel)
	{
		public static GameConfig Init = new(LastLevel: 1);
	}

	private readonly JsonSerializerSettings m_serializerSettings;
	private readonly Dictionary<int, LevelData> m_savedLevelDataDic;

	private GameConfig m_gameConfig;
	private LevelData? m_currentData;

	public LevelData? CurrentLevelData => m_currentData;

	public LevelDataManager()
	{
		m_gameConfig = LoadConfig();
		m_savedLevelDataDic = new();
		m_serializerSettings = new JsonSerializerSettings {
			Converters = new [] {
				new Vector3Converter()
			},
			Formatting = Formatting.Indented,
			ContractResolver = new UnityTypeContractResolver()
		};
	}

	public LevelData CreateLevelData()
	{
		int nextLevel = m_gameConfig.LastLevel + 1;
		m_gameConfig = m_gameConfig with { LastLevel = nextLevel };
		return LoadLevelData(nextLevel);
	}

	public LevelData LoadLevelData(int level)
	{
		string path = GetLevelDataPath(level);

		var data = m_savedLevelDataDic.ContainsKey(level)? 
			m_savedLevelDataDic[level] :
			File.Exists(path) switch {
				true => LoadInternal<LevelData>(path),
				_ => LevelData.CreateData(level)
			};

		data ??= LevelData.CreateData(level);

		if (!m_savedLevelDataDic.ContainsKey(level))
		{
			m_savedLevelDataDic.Add(level, data);
		}

		m_currentData = data;

		return data;
	}

	public LevelData? LoadLevelDataBy(int amount)
	{
		int selectLevel = Mathf.Max((m_currentData?.Level + amount) ?? 1, 1);
		return selectLevel > m_gameConfig.LastLevel? CreateLevelData() : LoadLevelData(selectLevel);
	}

	public GameConfig LoadConfig()
	{
		string path = GetGameConfigPath();
		GameConfig newConfig = GameConfig.Init;

		if (!File.Exists(path))
		{
			SaveInternal(path, newConfig);
			return newConfig;
		}
		
		return LoadInternal<GameConfig>(path) ?? GameConfig.Init;
	}

	public void SaveLevelData()
	{
		if (m_currentData == null)
		{
			return;
		}

		int level = m_currentData.Level;
		string path = GetLevelDataPath(level);

		SaveInternal(GetGameConfigPath(), m_gameConfig);
		SaveInternal(path, m_currentData);

		if (!m_savedLevelDataDic.ContainsKey(level))
		{
			m_savedLevelDataDic.Add(level, m_currentData);
		}
		else
		{
			m_savedLevelDataDic[level] = m_currentData;
		}
	}

	public int? UpdateNumberOfTypes(int number)
	{
		if (m_currentData == null)
		{
			return default;
		}

		int clamp = Mathf.Max(number, 1);

		var data = m_currentData with {
			NumberOfTileTypes = clamp
		};

		return clamp;
	}

	public bool TryAddTileData(Vector2 position)
	{
		Layer? layerData = m_currentData?.GetLayer(0, 0); //need to add arguments (boardIndex, LayerIndex)

		if (layerData?.Tiles?.All(tile => !position.Equals(tile.Position)) ?? false)
		{
			layerData.Tiles.Add(new Tile(0, position));
			return true;
		}

		return false;
	}

	public bool TryRemoveTileData(Vector2 position)
	{
		Layer? layerData = m_currentData?.GetLayer(0, 0); //need to add arguments (boardIndex, LayerIndex)

		if (layerData?.Tiles is var tiles and not null)
		{
			return tiles.RemoveAll(tile => tile.Position == position) > 0;
		}

		return false;
	}

	public void ClearTileDatasInLayer()
	{
		Layer? layerData = m_currentData?.GetLayer(0, 0); //need to add arguments (boardIndex, LayerIndex)

		if (layerData != null)
		{
			layerData.Tiles.Clear();
		}
	}

	private T? LoadInternal<T>(string path)
	{
		using (StreamReader sr = new StreamReader(path))
		{
			using (JsonReader reader = new JsonTextReader(sr))
			{
				string? json = reader.ReadAsString();

				if (string.IsNullOrEmpty(json))
				{
					return default(T);
				}

				return JsonConvert.DeserializeObject<T>(json, m_serializerSettings);
			}
		}
	}

	private void SaveInternal<T>(string path, T data)
	{
		if (data == null)
		{
			return;
		}

		using (StreamWriter sw = new StreamWriter(path))
		{
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				string json = JsonConvert.SerializeObject(data, m_serializerSettings);
				writer.WriteValue(json);
			}
		}
	}

	private string GetLevelDataPath(int level) => Path.Combine(Application.dataPath, $"LevelData_{level}.dat");
	private string GetGameConfigPath() => Path.Combine(Application.dataPath, "GameConfig.dat");
}
