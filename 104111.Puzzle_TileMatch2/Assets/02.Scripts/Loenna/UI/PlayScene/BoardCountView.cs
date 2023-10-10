using UnityEngine;

using System.Collections.Generic;

using NineTap.Common;

public class BoardCountView : CachedBehaviour
{
	[SerializeField]
	private BoardCountItem m_countItemPrefab;

	private List<BoardCountItem> m_cachedItems = new();

	private int m_currentIndex = -1;

	public void OnSetup(int startIndex, int boardCount)
	{
		if (startIndex < 0 || boardCount < 1)
		{
			return;
		}

		m_currentIndex = startIndex;

		for (int i = 0, count = Mathf.Max(boardCount, m_cachedItems.Count); i < count; i++)
		{
			if (i >= boardCount && m_cachedItems.HasIndex(i))
			{
				m_cachedItems[i].CachedGameObject.SetActive(false);
				continue;
			}

			(bool instantiate, BoardCountItem go) = m_cachedItems.HasIndex(i)? 
				(false, m_cachedItems[i]) : 
				(true, Instantiate<BoardCountItem>(m_countItemPrefab));

			go.transform.SetParent(CachedTransform, false);

			if (instantiate)
			{
				m_cachedItems.Add(go);
				go.OnSetup(i < boardCount - 1);
			}
			
			go.OnUpdateUI(i <= startIndex);
		}
	}

	public void OnUpdateUI(int playingIndex)
	{
		if (playingIndex == m_currentIndex)
		{
			return;
		}

		for (int i = 0, count = m_cachedItems.Count; i < count; i++)
		{
			var go = m_cachedItems[i];	
			go.OnUpdateUI(i <= playingIndex);
		}
	}
}
