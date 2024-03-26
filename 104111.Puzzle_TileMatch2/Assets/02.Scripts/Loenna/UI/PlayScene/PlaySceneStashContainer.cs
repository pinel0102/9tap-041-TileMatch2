using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using NineTap.Common;

public class PlaySceneStashContainer : CachedBehaviour
{
	[SerializeField]
	private RectTransform m_root;
	private List<StashLayer> m_cachedLayers = new();
	private Dictionary<Guid, Transform> m_cachedStashes = new();
    [SerializeField]
    private Transform[] m_topTiles = new Transform[Constant.Game.STASH_TILE_AMOUNT];
    public Transform[] TopTiles => m_topTiles;
    private const float stashHeight = 10f;

	public void Clear()
	{   
		m_cachedLayers.ForEach(layer => layer.gameObject.SetActive(false));
        
        ClearTopTiles();
	}

	public UniTask OnUpdateUI(TileItem[] tileItems)
	{
		int count = tileItems?.Length ?? 0;

		var removeKeys = count switch {
			<= 0 => new List<Guid>(m_cachedStashes.Keys),
			_ => new List<Guid>(
				m_cachedStashes
				.Where(pair => tileItems.All(item => item.Current?.Guid != pair.Key))
				.Select(pair => pair.Key)
			)
		};
		
		foreach (var key in removeKeys)
		{
			Transform itemTransform = m_cachedStashes[key];
			m_cachedLayers.ForEach(
				layer => {
					for (int i = 0; i < Constant.Game.STASH_TILE_AMOUNT; i++)
					{
						Transform slot = layer[i];

						if (slot == null) 
						{
							continue;
						}

						if (ReferenceEquals(slot, itemTransform))
						{
							layer[i] = null;
						}
					}
				}
			);
			m_cachedStashes.Remove(key);
		}

        RefreshTopTiles();

		return UniTask.WhenAll( 
				tileItems
				.Where(item => item?.Current != null && !m_cachedStashes.ContainsKey(item.Current.Guid))
				.Select( 
					item => {
                    
                    m_cachedStashes.Add(item.Current.Guid, item.CachedTransform);
                    (var direction, var layer, var slotIndex) = GetStashLayer(item);
                    
                    return item.OnChangeLocation(LocationType.STASH, direction, Constant.Game.TWEENTIME_TILE_DEFAULT, 
                        onComplete: () => {
                            SetParentLayer(item, layer, slotIndex);
                    });
				}
			)
		);
	}

    private void SetParentLayer(TileItem tileItem, StashLayer layer, int slotIndex)
    {
        tileItem.CachedTransform.SetParent(layer.CachedTransform, true);
        tileItem.CachedTransform.SetSiblingIndex(slotIndex);
        
        RefreshTopTiles();
    }

    private (Vector2, StashLayer, int) GetStashLayer(TileItem tileItem)
    {
        if (m_cachedLayers.Count > 0)
        {
            StashLayer layer = m_cachedLayers.FirstOrDefault(layer => layer.GetEmptySlot() > -1);
            if (layer != null)
            {
                layer.gameObject.SetActive(true);
                int slotIndex = layer.GetEmptySlot();
                layer[slotIndex] = tileItem.CachedTransform;
                return (GetDirection(layer, slotIndex), layer, slotIndex);
            }
        }

        StashLayer newLayer = CreateNewLayer();
        newLayer[0] = tileItem.CachedTransform;
        return (GetDirection(newLayer, 0), newLayer, 0);
    }

    private Vector2 GetDirection(StashLayer layer, int slotIndex)
    {
        return layer.transform.position + Vector3.right * slotIndex * Constant.Game.TILE_WIDTH * UIManager.SceneCanvas.scaleFactor;
    }

    private StashLayer CreateNewLayer()
    {
        StashLayer newLayer = Instantiate(ResourcePathAttribute.GetResource<StashLayer>());
        newLayer.name = $"Layer_{m_cachedLayers.Count}";
        newLayer.CachedTransform.SetParentReset(m_root, true);
        newLayer.CachedTransform.SetLocalPositionY(m_cachedLayers.Count * stashHeight);
        newLayer.CachedRectTransform.SetOffsetX(0, 0);
        m_cachedLayers.Add(newLayer);

        return newLayer;
    }

    private void ClearTopTiles()
    {
        for(int i=0; i < m_topTiles.Length; i++)
        {
            m_topTiles[i] = null;
        }
    }

    private void RefreshTopTiles()
    {
        ClearTopTiles();

        for(int i=0; i < Constant.Game.STASH_TILE_AMOUNT; i++)
        {
            for(int j = m_cachedLayers.Count - 1; j >= 0; j--)
            {
                if (m_topTiles[i] == null && m_cachedLayers[j][i] != null)
                {
                    m_topTiles[i] = m_cachedLayers[j][i];
                }
            }
        }
    }
}
