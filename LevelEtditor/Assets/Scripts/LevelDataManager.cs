using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;

public class LevelDataManager
{
	public record Current
	(
		int Level = 0,
		int BoardIndex = 0,
		int LayerIndex = 0
	);

	private readonly JsonSerializer m_serializer;

	private Dictionary<int, LevelData> m_levelDataDic;
	private Current m_current;

	public LevelDataManager()
	{
		m_current = new();
		m_levelDataDic = new();
		m_serializer = new JsonSerializer();
	}

	public void Load(int level)
	{
		string path = GetFilePath(level);
		if (!File.Exists(path))
		{
			return;
		}

		if (!m_levelDataDic.ContainsKey(level))
		{
			using (StreamReader sr = new StreamReader(path))
			{
				using (JsonReader reader = new JsonTextReader(sr))
				{
					m_levelDataDic.Add(level, m_serializer.Deserialize<LevelData>(reader));
				}
			}
		}

		m_current = new Current(level);
	}

	public void Save<T>(T data)
	{
		using (StreamWriter sw = new StreamWriter(GetFilePath(m_current.Level)))
		{
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				m_serializer.Serialize(writer, data, typeof(T));
			}
		}
	}

	public void AddTileData(Vector2 position)
	{
		if (m_levelDataDic.TryGetValue(m_current.Level, out LevelData levelData))
		{
			Layer layerData = levelData.GetLayer(m_current.BoardIndex, m_current.LayerIndex);

			if (layerData != null)
			{
				layerData.Tiles.Add(new Tile(0, position));
			}
		}
	}

	public void RemoveTileData()
	{
		// if (m_levelDataDic.TryGetValue(m_current.Level, out LevelData levelData))
		// {
		// 	Layer layerData = levelData.GetLayer(m_current.BoardIndex, m_current.LayerIndex);
		// }
	}

	public void ClearTileDatasInLayer()
	{
		if (m_levelDataDic.TryGetValue(m_current.Level, out LevelData levelData))
		{
			Layer layerData = levelData.GetLayer(m_current.BoardIndex, m_current.LayerIndex);

			if (layerData != null)
			{
				layerData.Tiles.Clear();
			}
		}
	}

	private string GetFilePath(int level) => Path.Combine(Application.dataPath, $"LevelData_{level}.dat");
}
