using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using NineTap.Common;

public class PlaySceneBoard : CachedBehaviour
{
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private RectTransform m_layerRoot;

	private List<PlaySceneLayer> m_cachedLayers = new();

	public CanvasGroup CanvasGroup
	{
		get
		{
			if (m_canvasGroup == null)
			{
				m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
			}

			return m_canvasGroup;
		}
	}

	public UniTask UpdateLayerAll(int layerCount, List<TileItem> tileItems, bool init = false)
	{
		if (layerCount <= 0)
		{
			return UniTask.CompletedTask;
		}

		for (int index = 0; index < layerCount; index++)
		{
			var items = tileItems
				.Where(item => item.Current?.LayerIndex == index)?
				.ToArray() ?? Array.Empty<TileItem>();
			
			Array.ForEach(
				items, 
				item => {
					UpdateLayer(index, item);
					item.CachedRectTransform.Reset();
					item.CachedRectTransform.SetLocalPosition(item.Current.Position);
				}
			);
		}

		if (init)
		{
			var tweens = m_cachedLayers.Select(
				async (layer, index) =>
				{
					layer.CanvasGroup.alpha = 0f;
					layer.CachedTransform.SetLocalPositionY(630f * UIManager.SceneCanvas.scaleFactor);
					await UniTask.Delay(100 * (index + 1));
					layer.CanvasGroup.alpha = 1f;
					await layer.CachedTransform.DOLocalMoveY(0f, 0.5f).SetEase(Ease.OutBack).ToUniTask();
				}
			);

			return UniTask.WhenAll(tweens);
		}

		return UniTask.CompletedTask;
	}

	public void UpdateLayer(int layerIndex, TileItem tileItem)
	{
		(var eachLayer, bool reusable) = m_cachedLayers.HasIndex(layerIndex)? 
		(
			m_cachedLayers[layerIndex], 
			true
		): 
		(
			Instantiate(ResourcePathAttribute.GetResource<PlaySceneLayer>()), 
			false
		);

		if (!reusable)
		{
			eachLayer.CachedTransform.SetParentReset(m_layerRoot, true);
			eachLayer.CachedRectTransform.SetStretch();
			m_cachedLayers.Add(eachLayer);
		}
	
		eachLayer.name = $"Layer_{layerIndex}";
		//eachLayer.CachedGameObject.SetActive(true);
		tileItem.CachedTransform.SetParent(eachLayer.CachedTransform);
	}
}
