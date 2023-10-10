#nullable enable

using UnityEngine;

using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using NineTap.Common;

public class PlaySceneBoardView : CachedBehaviour
{
	[SerializeField]
	private RectTransform m_content = null!;
	
	[SerializeField]
	private PlaySceneBoard m_currentBoard = null!;

	[SerializeField]
	private PlaySceneBoard m_nextBoard = null!;

	public PlaySceneBoard CurrentBoard => m_currentBoard;

	public async UniTask MoveNextAndChange(int layerCount, List<TileItem> tileItems)
	{
		m_currentBoard.CanvasGroup.alpha = 0f;
		await UniTask.Defer(() => m_nextBoard.UpdateLayerAll(layerCount, tileItems));
		await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(0.25f)));

		await UniTask.Defer(
			() => m_content.DOAnchorPosX(
				-m_currentBoard.CachedRectTransform!.rect.width, 
				1.5f
			).SetEase(Ease.OutQuart)
			.ToUniTask());
		await UniTask.Defer(() => UniTask.Delay(TimeSpan.FromSeconds(0.25f)));

		await UniTask.Defer(() => m_currentBoard.UpdateLayerAll(layerCount, tileItems));
		m_currentBoard.CanvasGroup.alpha = 1f;
		m_content.anchoredPosition = Vector2.zero;
		UniTask.Defer(() => m_nextBoard.UpdateLayerAll(0, null)).Forget();
	}

	public UniTask OnUpdateAll(int layerCount, List<TileItem> tileItems)
	{
		return m_currentBoard.UpdateLayerAll(layerCount, tileItems, true);
	}
}
