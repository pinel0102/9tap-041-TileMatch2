using UnityEngine;

using NineTap.Common;
using System.Collections.Generic;
using System.Linq;

[ResourcePath("UI/Widgets/Layer")]
public class PlaySceneLayer : CachedBehaviour
{
    public int layerIndex;
    [SerializeField] private List<TileItem> tileList = new List<TileItem>();

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
        tileList.Clear();

        foreach(Transform child in transform)
        {
            TileItem tile = child.GetComponent<TileItem>();
            if (tile != null)
                tileList.Add(tile);
        }

        tileList = tileList.OrderBy(tile => tile.siblingIndex).ToList();
        tileList.ForEach(tile => {
            tile.transform.SetAsLastSibling();
        });
    }
}
