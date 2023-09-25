using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

public class Table<KeyT, T> where T: TableRowData<KeyT>
{
	protected readonly IDataImporter<T> m_importer;
	protected readonly Dictionary<KeyT, T> m_rowDataDic = new();

	public Dictionary<KeyT, T> Dic => m_rowDataDic;

	protected static string m_resourcePath = string.Empty;
	public static string ResourcePath => m_resourcePath;

	/// <summary>
	/// rawData가 json이 아닐 경우 생성자로 importer정의
	/// </summary>
	/// <typeparam name="KeyT"></typeparam>
	/// <typeparam name="T"></typeparam>
	public Table(string path, IDataImporter<T> importer)
	{
		m_resourcePath = path;
		m_importer = importer;
	}

	public Table(string path)
	{
		m_resourcePath = path;
		m_importer = new JsonImporter<T>();
	}

	public T this[KeyT key]
	{
		get
		{
			if (m_rowDataDic.TryGetValue(key, out T value))
			{
				return value;
			}

			return default;
		}
	}

	public bool TryGetValue(KeyT key, out T value)
	{
		value = default;
		
		if (m_rowDataDic.ContainsKey(key))
		{
			value = m_rowDataDic[key];
			return true;
		}

		return false;
	}

	public T FirstOrDefault(Func<KeyT, bool> predicate = null)
	{
		if (predicate != null)
		{
			return m_rowDataDic?.FirstOrDefault(pair => predicate.Invoke(pair.Key)).Value ?? m_rowDataDic?.FirstOrDefault().Value ?? default(T);
		}
		
		return m_rowDataDic?.FirstOrDefault().Value ?? default(T);
	}

	public virtual async UniTask LoadAsync(string[] array, string index = "")
	{
		T[] datas = await m_importer.ImportAsync(array, index);
		LoadInternal(datas);
	}

	public virtual void Load()
	{
		TextAsset textAsset = Resources.Load<TextAsset>(m_resourcePath);
		T[] datas = m_importer.Import(textAsset);
		LoadInternal(datas);
	}

	protected virtual void LoadInternal(T[] rowDatas)
	{
		if (rowDatas == null)
		{
			return;
		}

		foreach (var result in rowDatas.Select(data => (T)data))
		{
			if (result != null)
			{
				if (!m_rowDataDic.ContainsKey(result.Key))
				{
					m_rowDataDic.Add(result.Key, default);
				}

				m_rowDataDic[result.Key] = result;
			}
		}

		OnLoaded();
	}

	protected virtual void OnLoaded() 
	{
		//foreach (var data in m_rowDataDic.Values)
		//{
		//	UnityEngine.Debug.LogWarning(data);
		//}
	}
}
