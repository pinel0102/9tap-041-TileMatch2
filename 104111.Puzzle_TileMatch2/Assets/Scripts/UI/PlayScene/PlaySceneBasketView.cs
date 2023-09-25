using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using NineTap.Common;

public class PlaySceneBasketView : CachedBehaviour
{
	[SerializeField]
	private LayoutGroup m_parent;

	private List<TileItem> m_tileItems = new();

	public void Clear()
	{
		m_tileItems.Clear();
	}

	public UniTask OnAddItemUI(TileItem tileItem)
	{
		if (tileItem?.Current?.Location is not LocationType.BASKET and not LocationType.POOL)
		{
			Debug.LogWarning($"ERROR {tileItem.Current?.Icon}");
			return UniTask.CompletedTask;
		}

		tileItem?.CachedRectTransform?.SetParent(m_parent.transform, true);
		int index = m_tileItems.FindLastIndex(item => item.Current?.Icon == tileItem.Current?.Icon);

		var insertIndex = index < 0? m_tileItems.Count : index + 1;
		m_tileItems.Insert(insertIndex, tileItem);

		return UniTask.Create(
			async () => {
				var tasks = m_tileItems
					.Select(
						(item, itemIndex) => {
							return item.OnChangeLocation(
								LocationType.BASKET, 
								m_parent.transform.position + Vector3.right * itemIndex * Constant.Game.TILE_WIDTH * UIManager.SceneCanvas!.scaleFactor,
								itemIndex == insertIndex? Constant.Game.TWEEN_DURATION_SECONDS : Constant.Game.DEFAULT_DURATION_SECONDS
							);
						}
					);
				await UniTask.WhenAll(tasks);
			}
		);
	}

	public UniTask OnRemoveItemUI(IEnumerable<TileItemModel> tiles, List<(Guid Guid, int Icon)> basket)
	{
		var items = m_tileItems.Where(item => tiles.Any(tile => tile.Guid == item.Current?.Guid));

		var removedTiles = new List<TileItem>(m_tileItems.FindAll(item => basket.All(b => b.Guid != item.Current.Guid)));

		if (removedTiles.Count <= 0)
		{
			return UniTask.CompletedTask;
		}

		m_tileItems.RemoveAll(item => basket.All(b => b.Guid != item.Current.Guid));
		
		var tasks = removedTiles.Select(
			tile => {
				LocationType locationType = tiles.FirstOrDefault(x => x.Guid == tile.Current?.Guid)?.Location ?? LocationType.POOL;
				if (locationType is not LocationType.STASH)
				{
					return UniTask.Defer(() => tile.OnChangeLocation(locationType));
				}
				return UniTask.CompletedTask;	
			}
		);

		var task = UniTask.Create(
			async () => {
				await UniTask.Defer(() => UniTask.WhenAll(tasks));
				if (m_tileItems.Count > 0)
				{
					await UniTask.Defer(
						() => UniTask.WhenAll(
							m_tileItems
							.Select(
								(item, itemIndex) => {
									return item.OnChangeLocation(
										item.Current.Location, 
										m_parent.transform.position + Vector3.right * itemIndex * Constant.Game.TILE_WIDTH * UIManager.SceneCanvas!.scaleFactor,
										Constant.Game.DEFAULT_DURATION_SECONDS
									);
								}
							)
						)
					);
				}
			}
		);

		return task;	
	}
}
