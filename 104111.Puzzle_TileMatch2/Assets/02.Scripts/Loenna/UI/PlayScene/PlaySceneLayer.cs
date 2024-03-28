using UnityEngine;

using NineTap.Common;
using System.Collections.Generic;
using System.Linq;

[ResourcePath("UI/Widgets/Layer")]
public class PlaySceneLayer : CachedBehaviour
{
    public int layerIndex;
    [SerializeField] private List<TileItem> m_tileList = new List<TileItem>();
    public List<TileItem> TileList => m_tileList;

	private CanvasGroup m_canvasGroup;
	public CanvasGroup CanvasGroup
	{
		get 
		{
			if (m_canvasGroup == null)
			{
				m_canvasGroup = CachedGameObject.AddComponent<CanvasGroup>();
			}
			return m_canvasGroup;
		}
	}

    public void SortChild()
    {
        m_tileList.Clear();

        foreach(Transform child in transform)
        {
            TileItem tile = child.GetComponent<TileItem>();
            if (tile != null)
                m_tileList.Add(tile);
        }

        m_tileList = m_tileList.OrderBy(tile => tile.siblingIndex).ToList();
        m_tileList.ForEach(tile => {
            tile.transform.SetAsLastSibling();
        });
    }
}
