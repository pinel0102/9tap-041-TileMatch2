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

	public void Clear()
	{
		m_cachedLayers.ForEach(layer => layer.gameObject.SetActive(false));
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

		return UniTask.WhenAll( 
				tileItems
				.Where(item => item?.Current != null && !m_cachedStashes.ContainsKey(item.Current.Guid))
				.Select( 
					item => {
					(Transform transform, int index) = SetParent(item);
					m_cachedStashes.Add(item.Current.Guid, item.CachedTransform);

					Vector2 moveAt = transform.position + Vector3.right * index * Constant.Game.TILE_WIDTH * UIManager.SceneCanvas.scaleFactor;

					return item.OnChangeLocation(LocationType.STASH, moveAt, Constant.Game.DEFAULT_DURATION_SECONDS);
				}
			)
		);

		(Transform, int index) SetParent(TileItem item)
		{
			if (m_cachedLayers.Count > 0)
			{
				StashLayer layer = m_cachedLayers.FirstOrDefault(layer => layer.CachedTransform.childCount < Constant.Game.REQUIRED_MATCH_COUNT);
				if (layer != null)
				{
					layer.gameObject.SetActive(true);
					item.CachedRectTransform.SetParent(layer.CachedTransform, true);
					int emptyIndex = layer.GetEmptySlot();
					layer[emptyIndex] = item.CachedTransform;
					return (layer.CachedTransform, emptyIndex);
				}
			}

			StashLayer newLayer = Instantiate(ResourcePathAttribute.GetResource<StashLayer>());
			newLayer.name = $"Layer_{m_cachedLayers.Count}";
			newLayer.CachedTransform.SetParentReset(m_root, true);
			newLayer.CachedTransform.SetLocalPositionY(m_cachedLayers.Count);
			newLayer.CachedRectTransform.SetOffsetX(0, 0);
			m_cachedLayers.Add(newLayer);
			item.CachedRectTransform.SetParent(newLayer.CachedTransform, true);
			newLayer[0] = item.CachedTransform;

			return (newLayer.CachedTransform, 0);
		}
	}
}
