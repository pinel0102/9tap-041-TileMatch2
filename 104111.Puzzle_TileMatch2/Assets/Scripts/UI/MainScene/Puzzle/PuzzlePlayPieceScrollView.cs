using UnityEngine;

using Gpm.Ui;

using NineTap.Common;

public class PuzzlePlayPieceScrollView : CachedBehaviour
{
	[SerializeField]
	private InfiniteScroll m_infiniteScroll;

	public void UpdateUI(PuzzlePieceItemData[] itemDatas)
	{
		m_infiniteScroll.InsertData(itemDatas);
	}

	public void UpdateUI(PuzzlePieceItemData data)
	{
		m_infiniteScroll.UpdateData(data);
	}

	public void RemoveItem(PuzzlePieceItemData itemData)
	{
		m_infiniteScroll.RemoveData(itemData);
	}

	public void TearDown()
	{
		m_infiniteScroll.ClearData();
	}
}
