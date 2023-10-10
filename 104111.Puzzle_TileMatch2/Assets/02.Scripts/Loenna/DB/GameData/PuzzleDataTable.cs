using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

public class PuzzleDataTable : Table<int, PuzzleData>
{
	private Dictionary<string, List<PuzzleData>> m_collections;

	public IReadOnlyList<string> CollectionKeys => m_collections?.Keys?.ToArray() ?? Array.Empty<string>();

	public PuzzleDataTable(string path) : base(path)
	{
		m_collections = new();
	}

    public override async UniTask LoadAsync(string[] array, string index = "")
	{
		JsonImporter<PiecesData> jsonImporter = new JsonImporter<PiecesData>();
		PiecesData[] datas = await jsonImporter.ImportAsync(array, index);

		if (datas != null)
		{	
			foreach (var result in datas)
			{
				if (result != null)
				{
					if (m_rowDataDic.TryGetValue(result.Index, out var rowData))
					{
						m_rowDataDic[result.Index] = rowData with { Pieces = result.Pieces };
					}
				}
			}
		}

		foreach (var (_, data) in m_rowDataDic)
		{
			string code = data.CountryCode;
			if (!m_collections.ContainsKey(code))
			{
				m_collections.Add(code, new());
			}

			m_collections[code].Add(data);
		}

		//foreach (var data in m_rowDataDic.Values)
		//{
		//	UnityEngine.Debug.LogWarning(data);
		//}
	}

	public List<PuzzleData> GetCollection(string countryCode)
	{
		if (m_collections.TryGetValue(countryCode, out var list))
		{
			return list;
		}

		return null;
	}
}
