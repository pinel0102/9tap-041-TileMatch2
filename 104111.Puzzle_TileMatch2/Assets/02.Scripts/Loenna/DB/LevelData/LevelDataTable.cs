public class LevelDataTable : Table<int, LevelData>
{
	public LevelDataTable() : base(string.Empty)
	{
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
