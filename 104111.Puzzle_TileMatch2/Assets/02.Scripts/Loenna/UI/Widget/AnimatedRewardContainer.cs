using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using DG.Tweening;

using NineTap.Common;

public class AnimatedRewardContainerParameter
{
	public List<IReward> Rewards = new();
	public Action OnFinishedAnimation;
}

public class AnimatedRewardContainer : CachedBehaviour
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private List<RewardGoodsItem> m_rewardGoodsItems = new();

	public float Alpha { set => m_canvasGroup.alpha = value; }

	public void OnSetup(AnimatedRewardContainerParameter parameter)
	{
		m_canvasGroup.alpha = 0f;

		var prefab = ResourcePathAttribute.GetResource<RewardGoodsItem>();

        var items = parameter.Rewards.Select(
			(reward, index) => {
				GameObject go = new GameObject($"Position {index}");
				LayoutElement layoutElement = go.AddComponent<LayoutElement>();
				layoutElement.minWidth = 120;

				go.transform.SetParentReset(CachedTransform);

				LayoutRebuilder.ForceRebuildLayoutImmediate(CachedRectTransform);

				var item = Instantiate(prefab);
				item.OnSetup(
					new RewardGoodsItemParameter{
						Animated = true,
						IconSize = 114,
						FontSize = 59
					}
				);
				item.CachedTransform.SetParentReset(go.transform);
				item.UpdateUI(
					reward.Type.GetIconName(), 
					reward.GetAmountString(),
					UIManager.HUD.GetAttractorTarget(reward.Type.GetHUDType()),
					parameter.OnFinishedAnimation
				);
				item.transform.localScale = Vector2.zero;

				return item;
			}
		);

		m_rewardGoodsItems.AddRange(items);
	}

	public async UniTask ShowAsync(CancellationToken token)
	{
		Vector2 from = CachedTransform.parent.position;
		m_rewardGoodsItems.ForEach(item => item.CachedTransform.position = from);

		await DOTween.To(
			getter: () => 0f,
			setter: alpha => Alpha = alpha,
			endValue: 1f,
			0.1f
		)
		.SetEase(Ease.OutQuad)
		.ToUniTask()
		.AttachExternalCancellation(token);

        var tasks = m_rewardGoodsItems.Select(
			item => UniTask.WhenAll(
				item.CachedRectTransform
					.DOAnchorPos(Vector2.zero, 0.3f)
					.SetEase(Ease.OutQuad)
					.ToUniTask(),
				item.CachedTransform
					.DOScale(1f, 0.3f)
					.SetEase(Ease.OutBack)
					.ToUniTask(),
				UniTask.Create(
					() => UniTask.WhenAll(
						UniTask.Delay(TimeSpan.FromSeconds(0.5f)),
						item.PlayAsync(token)
					)
				)
			)
		).ToUniTaskAsyncEnumerable();

		await UniTaskAsyncEnumerable.ForEachAwaitAsync(tasks, task => task, token).AttachExternalCancellation(token);
	}

	public void ShowParticle()
	{
        m_rewardGoodsItems.ForEach(item => item.ShowParticle());
	}
}
