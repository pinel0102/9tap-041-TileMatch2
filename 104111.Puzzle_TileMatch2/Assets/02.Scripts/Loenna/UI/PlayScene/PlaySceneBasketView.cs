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
    
    public int level;
    public bool isTutorialLevel;
    public bool isTutorialShowed;
    public int tutorialCheckCount;
    
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

        //Debug.Log(CodeManager.GetMethodName() + string.Format("m_tileItems.Count : {0}", m_tileItems.Count));

		return UniTask.Create(
			async () => {
				var tasks = m_tileItems
					.Select(
						(item, itemIndex) => {
							return item.OnChangeLocation(
								LocationType.BASKET, 
								m_parent.transform.position + Vector3.right * itemIndex * Constant.Game.TILE_WIDTH * UIManager.SceneCanvas!.scaleFactor,
								itemIndex == insertIndex? Constant.Game.TWEENTIME_TILE_DEFAULT : Constant.Game.TWEENTIME_TILE_DEFAULT
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

        /*Debug.Log(CodeManager.GetMethodName() + string.Format("removedTiles.Count : {0}", removedTiles.Count));
        foreach(var item in removedTiles)
        {
            Debug.Log(CodeManager.GetMethodName() + string.Format("removedTiles.name : {0}", item.gameObject.name));
        }*/

        SoundManager soundManager = Game.Inst?.Get<SoundManager>();
        soundManager?.PlayFx(Constant.Sound.SFX_TILE_MATCH);

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
                
                /*Debug.Log(CodeManager.GetMethodName() + string.Format("m_tileItems.Count : {0}", m_tileItems.Count));
                foreach(var item in m_tileItems)
                {
                    Debug.Log(CodeManager.GetMethodName() + string.Format("m_tileItems.name : {0}", item.gameObject.name));
                }*/

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
										Constant.Game.TWEENTIME_TILE_DEFAULT
									);
								}
							)
						)
					);
				}
			}
		);

        //Debug.Log(CodeManager.GetMethodName() + string.Format("m_tileItems.Count : {0}", m_tileItems.Count));

		return task;	
	}

    public void CheckTutorialBasket()
    {
        if(isTutorialLevel && !isTutorialShowed)
        {
            if(m_tileItems.Count >= tutorialCheckCount)
            {
                Debug.Log(CodeManager.GetMethodName() + string.Format("m_tileItems.Count : {0}", m_tileItems.Count));
                GlobalData.Instance.ShowTutorial_Play(level);
                isTutorialShowed = true;
            }
        }
    }

    public Vector3 GetBasketAnchorPosition()
    {
        RectTransform rt = (RectTransform)m_parent.transform;
        Vector3[] v = new Vector3[4];
        rt.GetWorldCorners(v);
        Vector3 result = (v[0] + v[1]) * 0.5f;

        //Debug.Log(CodeManager.GetMethodName() + result);

        return result;
    }
}
