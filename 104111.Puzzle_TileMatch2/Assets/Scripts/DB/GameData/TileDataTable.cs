using System.Linq;
using System;
using System.Collections.Generic;

public class TileDataTable : Table<int, TileData>
{
	public TileDataTable(string path) : base(path)
	{
	}

	public IList<int> GetIndexes(int level, string countryCode)
	{
		return m_rowDataDic?
			.Where(pair => pair.Key > 0 && 
				pair.Value.MinimumLevel <= level && 
				(pair.Value.CountryCode == CountryCodeDataTable.DEFAULT_CODE || pair.Value.CountryCode == countryCode)
			)
			.Select(pair => pair.Key)
			.ToArray() ?? Array.Empty<int>();
	}

	//protected override void OnLoaded()
	//{
	//	base.OnLoaded();

	//	foreach (var data in m_rowDataDic.Values)
	//	{
	//		UnityEngine.Debug.LogWarning(data.ToString());
	//	}
	//}
}
