using System.Linq;

public class CountryCodeDataTable : Table<int, CountryCodeData>
{
	public static readonly string DEFAULT_CODE = "default";

	public CountryCodeDataTable(string path) : base(path)
	{
	}

	public int GetIndex(string code)
	{
		return Dic?.Values?.FirstOrDefault(value => value?.Code == code)?.Index ?? -1;
	}

	public string GetCountryName(string code)
	{
		int index = GetIndex(code);

		if (index < 0)
		{
			return string.Empty;
		}

		return this[index].Name;
	}

	//protected override void OnLoaded() {
	//	foreach (var data in m_rowDataDic.Values)
	//	{
	//		UnityEngine.Debug.LogWarning(data);
	//	}
	//}
}
